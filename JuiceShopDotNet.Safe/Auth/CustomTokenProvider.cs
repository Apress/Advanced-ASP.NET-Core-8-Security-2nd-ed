using JuiceShopDotNet.Common.Cryptography;
using JuiceShopDotNet.Common.Cryptography.Hashing;
using JuiceShopDotNet.Safe.Data;
using JuiceShopDotNet.Safe.Data.EncryptedDataStore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace JuiceShopDotNet.Safe.Auth;

public class CustomTokenProvider : IUserTwoFactorTokenProvider<JuiceShopUser>
{
    private readonly string _connectionString;

    public CustomTokenProvider(IConfiguration _config)
    {
        _connectionString = _config.GetConnectionString("DefaultConnection");
    }

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<JuiceShopUser> manager, JuiceShopUser user)
    {
        return Task.FromResult(true);
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
}
