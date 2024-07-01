// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using JuiceShopDotNet.Unsafe.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace JuiceShopDotNet.Unsafe.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IUserStore<IdentityUser> _userStore;
    private readonly IUserEmailStore<IdentityUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly ApplicationDbContext _dbContext;

    public RegisterModel(
        UserManager<IdentityUser> userManager,
        IUserStore<IdentityUser> userStore,
        SignInManager<IdentityUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        _dbContext = dbContext;
    }

    [BindProperty]
    public AspNetUser Input { get; set; }

    public string ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }


    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            if (SetUpUser())
            {
                if (!_dbContext.Users.Any(u => u.NormalizedUserName == Input.NormalizedUserName))
                {
                    _dbContext.Users.Add(Input);
                    _dbContext.SaveChanges();

                    _logger.LogInformation("User created a new account with password.");

                    var user = _userManager.FindByEmailAsync(Input.Email).Result;

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Username already taken");
                }
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    //This method sets properties on the user object that the SignInManager and UserManager would have otherwise done
    //This is an awkward method necessary in order to keep the default ASP.NET login in most places but introduce Mass Assignment/Overposting here
    private bool SetUpUser()
    {
        Input.Id = Guid.NewGuid().ToString();
        Input.UserName = Input.Email;
        Input.NormalizedUserName = Input.UserName.ToUpperInvariant();
        Input.NormalizedEmail = Input.Email.ToUpperInvariant();
        Input.SecurityStamp = _userManager.GenerateNewAuthenticatorKey();
        Input.ConcurrencyStamp = _userManager.GenerateConcurrencyStampAsync(Input).Result;
        Input.PasswordHash = _userManager.PasswordHasher.HashPassword(Input, Input.Password);
        Input.LockoutEnabled = true;

        foreach (var validator in _userManager.PasswordValidators)
        {
            var result = validator.ValidateAsync(_userManager, null, Input.Password).Result;
            if (result != IdentityResult.Success)
            {
                ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }
        }

        return true;
    }

    private IUserEmailStore<IdentityUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<IdentityUser>)_userStore;
    }
}
