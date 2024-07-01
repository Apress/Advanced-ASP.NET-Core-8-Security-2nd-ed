// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using JuiceShopDotNet.Safe.Data;
using Microsoft.EntityFrameworkCore;

namespace JuiceShopDotNet.Safe.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<JuiceShopUser> _signInManager;
    private readonly UserManager<JuiceShopUser> _userManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly ApplicationDbContext _dbContext;

    public LoginModel(SignInManager<JuiceShopUser> signInManager, UserManager<JuiceShopUser> userManager, ILogger<LoginModel> logger, 
        IEmailSender emailSender, ApplicationDbContext dbContext)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _emailSender = emailSender;
        _dbContext = dbContext;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [FromForm]
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string ErrorMessage { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!CanAccessPage())
            return RedirectToPage("./Lockout");

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                var user = _userManager.FindByNameAsync(Input.Username).Result;
                var code = _userManager.GenerateTwoFactorTokenAsync(user, "email");
                _emailSender.SendEmailAsync(user.UserEmail, "Your MFA Code", $"Your MFA code is: {code.Result}");
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private bool CanAccessPage()
    {
        var sourceIp = HttpContext.Connection.RemoteIpAddress.ToString();

        //SqlQuery is smart enough to understand that interpolated string values should be treated as parameters, so this is safe from SQL injection attacks
        var failedUsernameCount = _dbContext.Database.SqlQuery<int>($"SELECT COUNT(1) AS Value FROM SecurityEvent WHERE DateCreated > {DateTime.UtcNow.AddDays(-1)} AND RequestIP = {sourceIp} AND EventID = {Logging.SecurityEvent.Authentication.USER_NOT_FOUND.EventId}").Single();

        var failedPasswordCount = _dbContext.Database.SqlQuery<int>($"SELECT COUNT(1) AS Value FROM SecurityEvent WHERE DateCreated > {DateTime.UtcNow.AddDays(-1)} AND RequestIP = {sourceIp} AND EventID = {Logging.SecurityEvent.Authentication.PASSWORD_MISMATCH.EventId}").Single();

        if (failedUsernameCount >= 5 || failedPasswordCount >= 20)
            return false;
        else
            return true;
    }

}
