using JuiceShopDotNet.Common.Cryptography.SymmetricEncryption;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JuiceShopDotNet.API.Data.Converters;

public class EncryptionConverter : ValueConverter<string, string>
{
    public EncryptionConverter(string encryptionKeyName, IEncryptionService encryptionService)
    : base(v => ToDatabase(v, encryptionKeyName, 1, encryptionService), v => FromDatabase(v, encryptionKeyName, encryptionService)) { }

    public static string ToDatabase(string value, string keyName, int keyIndex, IEncryptionService encryptionService)
    {
        return encryptionService.Encrypt(value, keyName, keyIndex);
    }

    public static string FromDatabase(string value, string keyName, IEncryptionService encryptionService)
    {
        return encryptionService.Decrypt(value, keyName);
    }
}
