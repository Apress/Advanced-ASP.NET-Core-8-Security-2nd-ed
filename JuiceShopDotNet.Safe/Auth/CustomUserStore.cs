using JuiceShopDotNet.Common.Cryptography;
using JuiceShopDotNet.Common.Cryptography.Hashing;
using JuiceShopDotNet.Safe.Cryptography;
using JuiceShopDotNet.Safe.Data;
using JuiceShopDotNet.Safe.Data.EncryptedDataStore;
using JuiceShopDotNet.Safe.Data.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace JuiceShopDotNet.Safe.Auth;

public class CustomUserStore : IUserStore<JuiceShopUser>, IUserEmailStore<JuiceShopUser>, IUserPasswordStore<JuiceShopUser>, IUserTwoFactorTokenProvider<JuiceShopUser>,
        IUserRoleStore<JuiceShopUser>, IUserLockoutStore<JuiceShopUser>, IUserSecurityStampStore<JuiceShopUser>, IUserTwoFactorRecoveryCodeStore<JuiceShopUser>,
        IUserTwoFactorStore<JuiceShopUser>
{
    private readonly string _connectionString;
    private readonly IHashingService _hashingService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IRemoteSensitiveDataStore _apiStorage;

    public CustomUserStore(IConfiguration _config, IHashingService hashingService, IHttpContextAccessor contextAccessor, IRemoteSensitiveDataStore apiStorage)
    {
        _connectionString = _config.GetConnectionString("DefaultConnection");
        _hashingService = hashingService;
        _contextAccessor = contextAccessor;
        _apiStorage = apiStorage;
    }

    public Task AddToRoleAsync(JuiceShopUser user, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<JuiceShopUser> manager, JuiceShopUser user)
    {
        return Task.FromResult(true);
    }

    public Task<int> CountCodesAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> CreateAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        var identifier = Guid.NewGuid();

        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "INSERT JuiceShopUser (PublicIdentifier, UserName, UserEmail, NormalizedUserEmail, DisplayName, UserEmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp) VALUES (@PublicIdentifier, @UserName, @UserEmail, @NormalizedUserEmail, @DisplayName, @UserEmailConfirmed, @PasswordHash, @SecurityStamp, @ConcurrencyStamp)";

                cmd.Parameters.AddWithValue("@PublicIdentifier", identifier);
                cmd.Parameters.AddWithValue("@UserName", _hashingService.CreateSaltedHash(user.UserName, KeyNames.JuiceShopUser_UserName_Salt, 1, HashingService.HashAlgorithm.SHA3_512));
                cmd.Parameters.AddWithValue("@UserEmail", _hashingService.CreateSaltedHash(user.UserEmail, KeyNames.JuiceShopUser_UserEmail_Salt, 1, HashingService.HashAlgorithm.SHA3_512));
                cmd.Parameters.AddWithValue("@NormalizedUserEmail", _hashingService.CreateSaltedHash(user.UserEmail, KeyNames.JuiceShopUser_NormalizedUserEmail_Salt, 1, HashingService.HashAlgorithm.SHA3_512));
                cmd.Parameters.AddWithValue("@DisplayName", user.DisplayName);
                cmd.Parameters.AddWithValue("@UserEmailConfirmed", user.UserEmailConfirmed);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@SecurityStamp", user.SecurityStamp.ToDBNullable());
                cmd.Parameters.AddWithValue("@ConcurrencyStamp", user.ConcurrencyStamp.ToDBNullable());

                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT JuiceShopUserID FROM JuiceShopUser WHERE PublicIdentifier = @PublicIdentifier";

                cmd.Parameters.AddWithValue("@PublicIdentifier", identifier);

                cn.Open();
                user.JuiceShopUserID = Convert.ToInt32(cmd.ExecuteScalar());
                cn.Close();
            }

            var encryptedUserInfo = new EncryptedJuiceShopUser();
            encryptedUserInfo.JuiceShopUserID = user.JuiceShopUserID;
            encryptedUserInfo.UserName = user.UserName;
            encryptedUserInfo.UserEmail = user.UserEmail;
            encryptedUserInfo.NormalizedUserEmail = user.NormalizedUserEmail;

            _apiStorage.SaveJuiceShopUser(encryptedUserInfo);
        }

        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        //Do nothing
    }

    public Task<JuiceShopUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        JuiceShopUser user = null;

        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT JuiceShopUserID, DisplayName, UserEmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp FROM JuiceShopUser WHERE NormalizedUserEmail = @NormalizedUserEmail";

                cmd.Parameters.AddWithValue("@NormalizedUserEmail", _hashingService.CreateSaltedHash(normalizedEmail, KeyNames.JuiceShopUser_NormalizedUserEmail_Salt, 1, HashingService.HashAlgorithm.SHA3_512));

                cn.Open();
                user = LoadUserFromReader(cmd);
                cn.Close();
            }
        }

        return Task.FromResult(user);
    }

    public Task<JuiceShopUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        JuiceShopUser user = null;

        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT JuiceShopUserID, DisplayName, UserEmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp FROM JuiceShopUser WHERE JuiceShopUserID = @JuiceShopUserID";

                cmd.Parameters.AddWithValue("@JuiceShopUserID", userId);

                cn.Open();
                user = LoadUserFromReader(cmd);
                cn.Close();
            }
        }

        return Task.FromResult(user);
    }

    public Task<JuiceShopUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        JuiceShopUser user = null;

        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT JuiceShopUserID, DisplayName, UserEmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp FROM JuiceShopUser WHERE UserName = @UserName";

                cmd.Parameters.AddWithValue("@UserName", _hashingService.CreateSaltedHash(normalizedUserName, KeyNames.JuiceShopUser_UserName_Salt, 1, HashingService.HashAlgorithm.SHA3_512));

                cn.Open();
                user = LoadUserFromReader(cmd);
                cn.Close();
            }
        }

        return Task.FromResult(user);
    }

    public Task<string> GenerateAsync(string purpose, UserManager<JuiceShopUser> manager, JuiceShopUser user)
    {
        var code = Randomizer.CreateRandomString(3);

        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "INSERT MfaCode (JuiceShopUserID, Purpose, MfaValue) VALUES (@JuiceShopUserID, @Purpose, @MfaValue)";

                cmd.Parameters.AddWithValue("@JuiceShopUserID", user.JuiceShopUserID);
                cmd.Parameters.AddWithValue("@Purpose", purpose);
                cmd.Parameters.AddWithValue("@MfaValue", code);

                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
        }

        return Task.FromResult(code);
    }

    public Task<int> GetAccessFailedCountAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        int lastAttempt = 0;
        int lastLogin = 0;

        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"SELECT ISNULL(MAX(RowNumber), 0) AS LastAttempt, ISNULL(MAX(CASE WHEN EventType = 'Success' THEN RowNumber ELSE 0 END), 0) AS LastSuccessfulLogin
                                        FROM(
                                        SELECT *, ROW_NUMBER() OVER(ORDER BY EventDate) AS RowNumber
                                        FROM LoginEvent
                                        WHERE EventType <> 'LockedOut' AND JuiceShopUserID = @JuiceShopUserID) q";

                cmd.Parameters.AddWithValue("@JuiceShopUserID", user.JuiceShopUserID);

                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lastAttempt = Convert.ToInt32(reader.GetInt64(0));
                        lastLogin = Convert.ToInt32(reader.GetInt64(1));
                    }
                }

                cn.Close();
            }
        }

        return Task.FromResult(lastAttempt - lastLogin);
    }

    public Task<string> GetEmailAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserEmail);
    }

    public Task<bool> GetEmailConfirmedAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserEmailConfirmed);
    }

    public Task<bool> GetLockoutEnabledAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        DateTime? lastLockedOut = null;
        int lockoutCount = 0;

        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"SELECT * FROM GetLockoutsForUser(@JuiceShopUserID)";

                cmd.Parameters.AddWithValue("@JuiceShopUserID", user.JuiceShopUserID);

                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lastLockedOut = reader.GetNullableDateTime(0);
                        lockoutCount = reader.GetInt32(1);
                    }
                }

                cn.Close();
            }
        }

        if (!lastLockedOut.HasValue || lockoutCount == 0)
            return Task.FromResult(new DateTimeOffset?(DateTime.Parse("1970-01-01")));

        var multiplier = 2 ^ (lockoutCount - 1);
        return Task.FromResult(new DateTimeOffset?(lastLockedOut.Value.AddMinutes(5 * multiplier)));
    }

    public Task<string> GetNormalizedEmailAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserEmail);
    }

    public Task<string> GetNormalizedUserNameAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task<string> GetPasswordHashAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<IList<string>> GetRolesAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        IList<string> roles = new List<string>();

        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT RoleName FROM SystemRole r INNER JOIN SystemUserRole sur ON r.SystemRoleID = sur.SystemRoleID WHERE sur.JuiceShopUserID = @UserID";

                cmd.Parameters.AddWithValue("@UserID", user.JuiceShopUserID);

                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(reader.GetString(0));
                    }
                }

                cn.Close();
            }
        }

        return Task.FromResult(roles);
    }

    public Task<string?> GetSecurityStampAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp);
    }

    public Task<bool> GetTwoFactorEnabledAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        //This may be a problem if you run DAST scans, which require a repeatable script to log in
        return Task.FromResult(true);
    }

    public Task<string> GetUserIdAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.JuiceShopUserID.ToString());
    }

    public Task<string> GetUserNameAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task<IList<JuiceShopUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasPasswordAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task<int> IncrementAccessFailedCountAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        int count = 0;

        using (var cn = new SqlConnection(_connectionString))
        {
            cn.Open();

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "INSERT LoginEvent (JuiceShopUserID, EventType, EventDate, SourceIP) VALUES (@JuiceShopUserID, 'Failed', @EventDate, @SourceIP)";

                cmd.Parameters.AddWithValue("@JuiceShopUserID", user.JuiceShopUserID);
                cmd.Parameters.AddWithValue("@EventDate", DateTime.UtcNow);

                if (_contextAccessor == null || _contextAccessor.HttpContext == null || _contextAccessor.HttpContext.Connection == null || _contextAccessor.HttpContext.Connection.RemoteIpAddress == null)
                    cmd.Parameters.AddWithValue("@SourceIP", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@SourceIP", _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());

                cmd.ExecuteNonQuery();
            }

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT dbo.GetFailedLoginCountForUser(@JuiceShopUserID)";

                cmd.Parameters.AddWithValue("@JuiceShopUserID", user.JuiceShopUserID);

                count = int.Parse(cmd.ExecuteScalar().ToString());
            }

            cn.Close();
        }

        return Task.FromResult(count);
    }

    public Task<bool> IsInRoleAsync(JuiceShopUser user, string roleName, CancellationToken cancellationToken)
    {
        var count = 0;

        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM SystemRole r INNER JOIN SystemUserRole sur ON r.SystemRoleID = sur.RoleID WHERE sur.UserID = @UserID AND r.SystemRoleName = @RoleName";

                cmd.Parameters.AddWithValue("@UserID", user.JuiceShopUserID);
                cmd.Parameters.AddWithValue("@RoleName", roleName);

                cn.Open();

                count = int.Parse(cmd.ExecuteScalar().ToString());

                cn.Close();
            }
        }

        var isInRole = count == 1;
        return Task.FromResult(isInRole);
    }

    public Task<bool> RedeemCodeAsync(JuiceShopUser user, string code, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task RemoveFromRoleAsync(JuiceShopUser user, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ReplaceCodesAsync(JuiceShopUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ResetAccessFailedCountAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        using (var cn = new SqlConnection(_connectionString))
        {
            cn.Open();

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "INSERT LoginEvent (JuiceShopUserID, EventType, EventDate, SourceIP) VALUES (@JuiceShopUserID, 'Success', @EventDate, @SourceIP)";

                cmd.Parameters.AddWithValue("@JuiceShopUserID", user.JuiceShopUserID);
                cmd.Parameters.AddWithValue("@EventDate", DateTime.UtcNow);

                if (_contextAccessor == null || _contextAccessor.HttpContext == null || _contextAccessor.HttpContext.Connection == null || _contextAccessor.HttpContext.Connection.RemoteIpAddress == null)
                    cmd.Parameters.AddWithValue("@SourceIP", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@SourceIP", _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());

                cmd.ExecuteNonQuery();
            }

            cn.Close();
        }

        return Task.CompletedTask;
    }

    public Task SetEmailAsync(JuiceShopUser user, string email, CancellationToken cancellationToken)
    {
        user.UserEmail = email;
        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(JuiceShopUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.UserEmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task SetLockoutEnabledAsync(JuiceShopUser user, bool enabled, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SetLockoutEndDateAsync(JuiceShopUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        using (var cn = new SqlConnection(_connectionString))
        {
            cn.Open();

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "INSERT LoginEvent (JuiceShopUserID, EventType, EventDate, SourceIP) VALUES (@JuiceShopUserID, 'Lockout', @EventDate, @SourceIP)";

                cmd.Parameters.AddWithValue("@JuiceShopUserID", user.JuiceShopUserID);
                cmd.Parameters.AddWithValue("@EventDate", DateTime.UtcNow);

                if (_contextAccessor == null || _contextAccessor.HttpContext == null || _contextAccessor.HttpContext.Connection == null || _contextAccessor.HttpContext.Connection.RemoteIpAddress == null)
                    cmd.Parameters.AddWithValue("@SourceIP", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@SourceIP", _contextAccessor.HttpContext.Connection.RemoteIpAddress);

                cmd.ExecuteNonQuery();
            }

            cn.Close();
        }

        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(JuiceShopUser user, string normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedUserEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task SetNormalizedUserNameAsync(JuiceShopUser user, string normalizedName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SetPasswordHashAsync(JuiceShopUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task SetSecurityStampAsync(JuiceShopUser user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task SetTwoFactorEnabledAsync(JuiceShopUser user, bool enabled, CancellationToken cancellationToken)
    {
        //All users should use MFA
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(JuiceShopUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(JuiceShopUser user, CancellationToken cancellationToken)
    {
        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "UPDATE JuiceShopUser SET UserName = @UserName, UserEmail = @UserEmail, NormalizedUserEmail = @NormalizedUserEmail, DisplayName = @DisplayName, UserEmailConfirmed = @UserEmailConfirmed, PasswordHash = @PasswordHash, SecurityStamp = @SecurityStamp, ConcurrencyStamp = @ConcurrencyStamp WHERE JuiceShopUserID = @JuiceShopUserID";

                cmd.Parameters.AddWithValue("@JuiceShopUserID", user.JuiceShopUserID);
                cmd.Parameters.AddWithValue("@UserName", _hashingService.CreateSaltedHash(user.UserName, KeyNames.JuiceShopUser_UserName_Salt, 1, HashingService.HashAlgorithm.SHA3_512));
                cmd.Parameters.AddWithValue("@UserEmail", _hashingService.CreateSaltedHash(user.UserEmail, KeyNames.JuiceShopUser_UserEmail_Salt, 1, HashingService.HashAlgorithm.SHA3_512));
                cmd.Parameters.AddWithValue("@NormalizedUserEmail", _hashingService.CreateSaltedHash(user.NormalizedUserEmail, KeyNames.JuiceShopUser_UserEmail_Salt, 1, HashingService.HashAlgorithm.SHA3_512));
                cmd.Parameters.AddWithValue("@DisplayName", user.DisplayName);
                cmd.Parameters.AddWithValue("@UserEmailConfirmed", user.UserEmailConfirmed);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@SecurityStamp", user.SecurityStamp.ToDBNullable());
                cmd.Parameters.AddWithValue("@ConcurrencyStamp", user.ConcurrencyStamp.ToDBNullable());

                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }

            var encryptedUserInfo = new EncryptedJuiceShopUser();
            encryptedUserInfo.JuiceShopUserID = user.JuiceShopUserID;
            encryptedUserInfo.UserName = user.UserName;
            encryptedUserInfo.UserEmail = user.UserEmail;
            encryptedUserInfo.NormalizedUserEmail = user.NormalizedUserEmail;
        }

        return Task.FromResult(IdentityResult.Success);
    }

    public Task<bool> ValidateAsync(string purpose, string token, UserManager<JuiceShopUser> manager, JuiceShopUser user)
    {
        var isValid = false;

        //TODO: Add expiration
        using (var cn = new SqlConnection(_connectionString))
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM MfaCode WHERE MfaValue = @Code AND JuiceShopUserID = @UserID AND Purpose = @Purpose";

                cmd.Parameters.AddWithValue("@UserID", user.JuiceShopUserID);
                cmd.Parameters.AddWithValue("@Purpose", purpose);
                cmd.Parameters.AddWithValue("@Code", token);

                cn.Open();

                isValid = int.Parse(cmd.ExecuteScalar().ToString()) > 0;

                cn.Close();
            }
        }

        return Task.FromResult(isValid);
    }

    private JuiceShopUser LoadUserFromReader(SqlCommand cmd)
    {
        JuiceShopUser? user = null;

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                if (user != null)
                    throw new InvalidOperationException("Found multiple users");

                user = new JuiceShopUser();

                user.JuiceShopUserID = reader.GetInt32(0);
                user.DisplayName = reader.GetString(1);
                user.UserEmailConfirmed = reader.GetBoolean(2);
                user.PasswordHash = reader.GetString(3);
                user.SecurityStamp = reader.GetNullableString(4);
                user.ConcurrencyStamp = reader.GetNullableString(5);
            }
        }

        if (user == null)
            return user;

        var userInfo = _apiStorage.GetJuiceShopUser(user.JuiceShopUserID);
        user.UserName = userInfo.UserName;
        user.UserEmail = userInfo.UserEmail;
        user.NormalizedUserEmail = userInfo.NormalizedUserEmail;

        return user;
    }
}
