using Humanizer.Localisation;
using JuiceShopDotNet.Safe.Cryptography.Hashing;
using JuiceShopDotNet.Safe.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace JuiceShopDotNet.Safe.Auth;

public class CustomUserManager : UserManager<JuiceShopUser>
{
    private readonly IServiceProvider _services;
    public CustomUserManager(IUserStore<JuiceShopUser> store, 
                             IOptions<IdentityOptions> optionsAccessor, 
                             IPasswordHasher<JuiceShopUser> passwordHasher, 
                             IEnumerable<IUserValidator<JuiceShopUser>> userValidators, 
                             IEnumerable<IPasswordValidator<JuiceShopUser>> passwordValidators, 
                             ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
                             IServiceProvider services, 
                             ILogger<UserManager<JuiceShopUser>> logger) 
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _services = services;
    }

    //Security fix - don't normalize the username for case-sensitive logins
    [return: NotNullIfNotNull("name")]
    public override string? NormalizeName(string? name)
    {
        return name;
    }

    public override async Task<JuiceShopUser?> FindByNameAsync(string userName)
    {
        ThrowIfDisposed();

        userName = NormalizeName(userName);

        var user = await Store.FindByNameAsync(userName, CancellationToken).ConfigureAwait(false);

        // Need to potentially check all keys
        if (user == null && Options.Stores.ProtectPersonalData)
        {
            var keyRing = _services.GetService<ILookupProtectorKeyRing>();
            var protector = _services.GetService<ILookupProtector>();
            if (keyRing != null && protector != null)
            {
                foreach (var key in keyRing.GetAllKeyIds())
                {
                    var oldKey = protector.Protect(key, userName);
                    user = await Store.FindByNameAsync(oldKey, CancellationToken).ConfigureAwait(false);
                    if (user != null)
                    {
                        return user;
                    }
                }
            }
        }
        return user;
    }

    public override async Task<bool> CheckPasswordAsync(JuiceShopUser user, string password)
    {
        ThrowIfDisposed();
        var passwordStore = GetPasswordStore();

        //Security fix: Commented out to fix timing-based user enumeration attacks
        //if (user == null)
        //{
        //    return false;
        //}

        var result = await VerifyPasswordAsync(passwordStore, user, password).ConfigureAwait(false);
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            await UpdatePasswordHash(user, password, validatePassword: false).ConfigureAwait(false);
            await UpdateUserAsync(user).ConfigureAwait(false);
        }

        var success = result != PasswordVerificationResult.Failed;
        if (!success)
        {
            Logger.LogDebug(0, "Invalid password for user.");
        }
        return success;
    }

    public override Task<IdentityResult> RemoveFromRoleAsync(JuiceShopUser user, string role)
    {
        var result = base.RemoveFromRoleAsync(user, role).Result;

        //Security fix: Invalidate security stamp with roles stored if a user is removed from a role
        if (result == IdentityResult.Success)
            this.UpdateSecurityStampAsync(user).Wait();

        return Task.FromResult(result);
    }

    public override Task<IdentityResult> RemoveFromRolesAsync(JuiceShopUser user, IEnumerable<string> roles)
    {
        var result = base.RemoveFromRolesAsync(user, roles).Result;

        //Security fix: Invalidate security stamp with roles stored if a user is removed from a role
        if (result == IdentityResult.Success)
            this.UpdateSecurityStampAsync(user).Wait();

        return Task.FromResult(result);
    }

    protected override async Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<JuiceShopUser> store, JuiceShopUser user, string password)
    {
        if (user != null)
        {
            var hash = await store.GetPasswordHashAsync(user, CancellationToken).ConfigureAwait(false);
            if (hash == null)
            {
                return PasswordVerificationResult.Failed;
            }
            return PasswordHasher.VerifyHashedPassword(user, hash, password);
        }
        else //Security fix: Hash a password anyway to fix timing-based user enumeration vulnerability
        {
            if (PasswordHasher is PasswordHashingService hashingService)
            {
                var validHexPassword = "D2C431A24632765498BD3972D46C946F84B9C6B29C3A618EDD54AD183CFAF1CEF7080BB6A5012CC316616F1959165ED21AE261D82F06D9EBA094F17ACAE281BD81CCB28ADCA3E47E038D9D4701A2F3D1B00A25A7F23186F78BDECACF729A70F7";
                hashingService.VerifyHashedPassword(user, hashingService.GetPrefixWithDefaults() + validHexPassword, password);
            }
            else
            {
                var validBase64Password = "AQAAAAIAAYagAAAAEOEjHkrWGDXiRmmO0ZhNQx1Y8fjeVwQgHgmaNj56GQJb4zSr+/E7KQAjrcHXM2IiTA==";
                PasswordHasher.VerifyHashedPassword(user, validBase64Password, password);
            }

            return PasswordVerificationResult.Failed;
        }
    }

    private IUserPasswordStore<JuiceShopUser> GetPasswordStore()
    {
        var cast = Store as IUserPasswordStore<JuiceShopUser>;
        if (cast == null)
        {
            throw new NotSupportedException("StoreNotIUserPasswordStore");
        }
        return cast;
    }
}
