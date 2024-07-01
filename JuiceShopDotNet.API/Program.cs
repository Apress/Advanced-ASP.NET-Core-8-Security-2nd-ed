using JuiceShopDotNet.API;
using JuiceShopDotNet.API.Cryptography;
using JuiceShopDotNet.API.Data;
using JuiceShopDotNet.Common.Cryptography.AsymmetricEncryption;
using JuiceShopDotNet.Common.Cryptography.KeyStorage;
using JuiceShopDotNet.Common.Cryptography.SymmetricEncryption;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Pkix;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton<ISignatureService, SignatureService>();
builder.Services.AddSingleton<ISecretStore, ForDemoPurposesOnlySecretStore>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://opperis.com",
        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) => {
            var keyService = builder.Services.BuildServiceProvider().GetRequiredService<ISecretStore>();
            var keyAsBytes = Encoding.UTF8.GetBytes(keyService.GetKey("JWTKey", 1));
            var key = new SymmetricSecurityKey(keyAsBytes);

            return new List<SecurityKey>() { key };
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
