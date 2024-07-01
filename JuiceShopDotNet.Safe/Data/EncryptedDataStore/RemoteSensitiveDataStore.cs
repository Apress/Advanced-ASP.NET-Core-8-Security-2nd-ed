using JuiceShopDotNet.Common.Cryptography.AsymmetricEncryption;
using System.Text;
using JuiceShopDotNet.Safe.Cryptography;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using JuiceShopDotNet.Common.Cryptography.KeyStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JuiceShopDotNet.Safe.Data.EncryptedDataStore;

public class RemoteSensitiveDataStore : IRemoteSensitiveDataStore
{
    private readonly IConfiguration _config;
    private readonly ISignatureService _signatureService;
    private readonly ISecretStore _secretStore;

    public RemoteSensitiveDataStore(IConfiguration config, ISignatureService signatureService, ISecretStore secretStore)
    { 
        _config = config;
        _signatureService = signatureService;
        _secretStore = secretStore;
    }

    public EncryptedCreditApplication GetCreditApplication(int id)
    {
        var data = new { id };
        var response = PostData(data, "GetCreditApplication");

        if (response.IsSuccessStatusCode)
        {
            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            return JsonSerializer.Deserialize<EncryptedCreditApplication>(response.Content.ReadAsStringAsync().Result, options);
        }
        else
        {
            //TODO: Log this
            return null;
        }
    }

    public EncryptedJuiceShopUser GetJuiceShopUser(int id)
    {
        var data = new { id };
        var response = PostData(data, "GetJuiceShopUser");

        if (response.IsSuccessStatusCode)
        {
            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            return JsonSerializer.Deserialize<EncryptedJuiceShopUser>(response.Content.ReadAsStringAsync().Result, options);
        }
        else
        {
            //TODO: Log this
            return null;
        }
    }

    public bool SaveCreditApplication(EncryptedCreditApplication application)
    {
        var response = PostData(application, "SaveCreditApplication");
        return response.IsSuccessStatusCode;
    }

    public bool SaveJuiceShopUser(EncryptedJuiceShopUser user)
    {
        var response = PostData(user, "SaveJuiceShopUser");
        return response.IsSuccessStatusCode;
    }

    private HttpResponseMessage PostData(object data, string endpoint)
    {
        try
        {
            var objectAsString = System.Text.Json.JsonSerializer.Serialize(data);
            var timestamp = DateTime.UtcNow;

            var signature = _signatureService.CreateSignature($"{timestamp}|{objectAsString}", KeyNames.ApiPrivateKey, 1, SignatureService.SignatureAlgorithm.RSA2048SHA512);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Timestamp", timestamp.ToString());
            client.DefaultRequestHeaders.Add("Signature", signature);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GetJwtToken()}");

            var content = new StringContent(objectAsString, Encoding.UTF8, "application/json");
            return client.PostAsync(new Uri(_config.GetValue<string>("EncryptionApiUrl") + "/Vault/" + endpoint), content).Result;
        }
        catch (Exception e)
        {
            //TODO: Log this
            return null;
        }
    }

    private string GetJwtToken()
    {
        var keyAsBytes = Encoding.UTF8.GetBytes(_secretStore.GetKey("JWTKey", 1));
        var key = new SymmetricSecurityKey(keyAsBytes);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var claims = new List<Claim>();
        claims.Add(new Claim("Purpose", "Demo"));
        var token = new JwtSecurityToken(issuer: "https://opperis.com", audience: null, claims, expires: DateTime.Now.AddMinutes(1), signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenString;
    }
}
