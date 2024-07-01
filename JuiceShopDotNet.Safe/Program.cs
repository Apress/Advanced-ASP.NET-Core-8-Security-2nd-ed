using JuiceShopDotNet.Common.Cryptography.AsymmetricEncryption;
using JuiceShopDotNet.Common.Cryptography.Hashing;
using JuiceShopDotNet.Common.Cryptography.KeyStorage;
using JuiceShopDotNet.Safe.Auth;
using JuiceShopDotNet.Safe.Cryptography;
using JuiceShopDotNet.Safe.Cryptography.Hashing;
using JuiceShopDotNet.Safe.CSP;
using JuiceShopDotNet.Safe.CSRF;
using JuiceShopDotNet.Safe.Data;
using JuiceShopDotNet.Safe.Data.EncryptedDataStore;
using JuiceShopDotNet.Safe.Emails;
using JuiceShopDotNet.Safe.Errors;
using JuiceShopDotNet.Safe.Logging;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<JuiceShopUser, SystemRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
    options.Tokens.AuthenticatorIssuer = "email";
    options.Tokens.AuthenticatorTokenProvider = "email";
    options.Tokens.EmailConfirmationTokenProvider = "email";
    options.Tokens.PasswordResetTokenProvider = "email";
    options.Tokens.PasswordResetTokenProvider = "email";
    options.Tokens.ChangePhoneNumberTokenProvider = "email";
})
    .AddTokenProvider<CustomTokenProvider>("email");

builder.Services.AddAuthentication();

builder.Services.AddSingleton<IRoleStore<SystemRole>, CustomRoleStore>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IHashingService, HashingService>();
builder.Services.AddSingleton<ISecretStore, ForDemoPurposesOnlySecretStore>();
builder.Services.AddSingleton<IRemoteSensitiveDataStore, RemoteSensitiveDataStore>();
builder.Services.AddSingleton<ISignatureService, SignatureService>();

builder.Services.RemoveAll<IUserStore<JuiceShopUser>>();
builder.Services.AddSingleton<IUserStore<JuiceShopUser>, CustomUserStore>();

builder.Services.RemoveAll<IPasswordHasher<JuiceShopUser>>();
builder.Services.AddSingleton<IPasswordHasher<JuiceShopUser>, PasswordHashingService>();

builder.Services.RemoveAll<SignInManager<JuiceShopUser>>();
builder.Services.AddScoped<SignInManager<JuiceShopUser>, CustomSignInManager>();

builder.Services.RemoveAll<UserManager<JuiceShopUser>>();
builder.Services.AddScoped<UserManager<JuiceShopUser>, CustomUserManager>();

builder.Services.RemoveAll<IAntiforgeryAdditionalDataProvider>();
builder.Services.AddSingleton<IAntiforgeryAdditionalDataProvider, AntiforgeryAdditionalDataProvider>();

builder.Services.RemoveAll<IEmailSender>();
builder.Services.RemoveAll<IEmailSender<JuiceShopUser>>();
builder.Services.AddSingleton<IEmailSender, EmailSimulatorToFile>();
builder.Services.AddSingleton<IEmailSender<JuiceShopUser>, EmailSimulatorToFile>();

builder.Services.AddSingleton<ISecurityLoggerFactory, SecurityLoggerFactory>();

builder.Services.ConfigureApplicationCookie(options => {
    options.AccessDeniedPath = "/Auth/MyAccount/AccessDenied";
    options.LoginPath = "/Auth/MyAccount/Login";
    options.LogoutPath = "/Auth/MyAccount/Login";
    
    options.Events = new CustomCookieAuthenticationEvents();
});

builder.Services.Configure<IdentityOptions>(options => { 
    options.User.RequireUniqueEmail = true;

    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 15;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

builder.Services.AddExceptionHandler<ErrorLogger>();

builder.Services.AddHsts(o =>
{
    o.Preload = true;
    o.IncludeSubDomains = true;
    o.MaxAge = TimeSpan.FromDays(365);
});

builder.Services.AddScoped<NonceContainer>();

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    Secure = CookieSecurePolicy.Always,
    HttpOnly = HttpOnlyPolicy.Always
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}").RequireAuthorization();

app.MapRazorPages().RequireAuthorization();

app.Use(async (context, next) => {
    context.Response.OnStarting(() => {
        context.Response.Headers["X-Frame-Options"] = "DENY";

        var nonceService = context.RequestServices.GetService<NonceContainer>();
        context.Response.Headers["Content-Security-Policy"] = $"default-src 'self'; script-src 'self' 'nonce-{nonceService.ID}'";
        return Task.FromResult(0);
    });

    await next();
});

app.Run();
