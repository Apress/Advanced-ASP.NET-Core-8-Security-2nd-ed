using JuiceShopDotNet.Safe.Data;
using JuiceShopDotNet.Safe.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;

namespace JuiceShopDotNet.Safe.Auth;

public class CustomSignInManager : SignInManager<JuiceShopUser>
{
    private readonly ISecurityLogger _securityLogger;

    public CustomSignInManager(UserManager<JuiceShopUser> userManager, 
                               IHttpContextAccessor contextAccessor, 
                               IUserClaimsPrincipalFactory<JuiceShopUser> claimsFactory, 
                               IOptions<IdentityOptions> optionsAccessor, 
                               ILogger<SignInManager<JuiceShopUser>> logger, 
                               IAuthenticationSchemeProvider schemes, 
                               IUserConfirmation<JuiceShopUser> confirmation,
                               ISecurityLoggerFactory securityLoggerFactory) 
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _securityLogger = securityLoggerFactory.CreateLogger<CustomSignInManager>();
    }

    public override async Task<SignInResult> PasswordSignInAsync(string userName, string password,
        bool isPersistent, bool lockoutOnFailure)
    {
        var user = await UserManager.FindByNameAsync(userName);

        //Security fix: Commented out to fix timing-based user enumeration attacks
        if (user == null)
        {
            //return SignInResult.Failed;
            _securityLogger.Log(SecurityEvent.Authentication.USER_NOT_FOUND, "User not found");
        }

        var result = await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);

        if (result.Succeeded)
        {
            string? userID = user == null ? null : UserManager.GetUserIdAsync(user).Result;
            _securityLogger.Log(SecurityEvent.Authentication.LOGIN_SUCCESSFUL, "User successfully logged in.", userID);
        }

        return result;
    }

    public override async Task<SignInResult> PasswordSignInAsync(JuiceShopUser user, string password,
        bool isPersistent, bool lockoutOnFailure)
    {
        //Security fix: Commented out to fix timing-based user enumeration attacks
        //ArgumentNullException.ThrowIfNull(user);

        var attempt = await CheckPasswordSignInAsync(user, password, lockoutOnFailure);
        return attempt.Succeeded
            ? await SignInOrTwoFactorAsync(user, isPersistent)
            : attempt;
    }

    public virtual async Task<SignInResult> CheckPasswordSignInAsync(JuiceShopUser user, string password, bool lockoutOnFailure)
    {
        //Security fix: Commented out to fix timing-based user enumeration attacks
        //ArgumentNullException.ThrowIfNull(user);

        //Security fix: Add null check before doing the PreSignInCheck
        if (user != null)
        {
            var error = await PreSignInCheck(user);
            if (error != null)
            {
                return error;
            }
        }

        if (await UserManager.CheckPasswordAsync(user, password))
        {
            var alwaysLockout = AppContext.TryGetSwitch("Microsoft.AspNetCore.Identity.CheckPasswordSignInAlwaysResetLockoutOnSuccess", out var enabled) && enabled;
            // Only reset the lockout when not in quirks mode if either TFA is not enabled or the client is remembered for TFA.
            if (alwaysLockout || !await IsTwoFactorEnabledAsync(user) || await IsTwoFactorClientRememberedAsync(user))
            {
                var resetLockoutResult = await ResetLockoutWithResult(user);
                if (!resetLockoutResult.Succeeded)
                {
                    // ResetLockout got an unsuccessful result that could be caused by concurrency failures indicating an
                    // attacker could be trying to bypass the MaxFailedAccessAttempts limit. Return the same failure we do
                    // when failing to increment the lockout to avoid giving an attacker extra guesses at the password.
                    return SignInResult.Failed;
                }
            }

            return SignInResult.Success;
        }

        if (user != null) 
        { 
            //Logger.LogDebug(2, "User failed to provide the correct password.");
            string? userID = UserManager.GetUserIdAsync(user).Result;
            _securityLogger.Log(SecurityEvent.Authentication.PASSWORD_MISMATCH, "User failed to provide the correct password.", userID);        
        }

        if (UserManager.SupportsUserLockout && lockoutOnFailure)
        {
            // If lockout is requested, increment access failed count which might lock out the user
            var incrementLockoutResult = await UserManager.AccessFailedAsync(user) ?? IdentityResult.Success;
            if (!incrementLockoutResult.Succeeded)
            {
                // Return the same failure we do when resetting the lockout fails after a correct password.
                return SignInResult.Failed;
            }

            if (await UserManager.IsLockedOutAsync(user))
            {
                return await LockedOut(user);
            }
        }
        return SignInResult.Failed;
    }

    //Copy/paste private method from source code to make minimal changes to code we do change
    private async Task<IdentityResult> ResetLockoutWithResult(JuiceShopUser user)
    {
        // Avoid relying on throwing an exception if we're not in a derived class.
        if (GetType() == typeof(SignInManager<JuiceShopUser>))
        {
            if (!UserManager.SupportsUserLockout)
            {
                return IdentityResult.Success;
            }

            return await UserManager.ResetAccessFailedCountAsync(user) ?? IdentityResult.Success;
        }

        try
        {
            var resetLockoutTask = ResetLockout(user);

            if (resetLockoutTask is Task<IdentityResult> resultTask)
            {
                return await resultTask ?? IdentityResult.Success;
            }

            await resetLockoutTask;
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            return IdentityResult.Failed();
        }
    }

    public override Task SignOutAsync()
    {
        var user = UserManager.GetUserAsync(base.Context.User).Result;

        if (user != null)
        {
            UserManager.UpdateSecurityStampAsync(user);
            string? userID = user == null ? null : UserManager.GetUserIdAsync(user).Result;
            _securityLogger.Log(SecurityEvent.Authentication.LOGOUT_SUCCESSFUL, "User logged out.", userID);
        }

        return base.SignOutAsync();
    }

    protected override Task<SignInResult> LockedOut(JuiceShopUser user)
    {
        //Logger.LogDebug(EventIds.UserLockedOut, "User is currently locked out.");
        string? userID = user == null ? null : UserManager.GetUserIdAsync(user).Result;
        _securityLogger.Log(SecurityEvent.Authentication.USER_LOCKED_OUT, "User is currently locked out.", userID);
        return Task.FromResult(SignInResult.LockedOut);
    }
}
