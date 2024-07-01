using JuiceShopDotNet.Common.Cryptography.Hashing;
using JuiceShopDotNet.Common.Cryptography.SymmetricEncryption;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JuiceShopDotNet.Safe.Data.ValueConverters;

public class IntegrityHashConverter : ValueConverter<string, string>
{
    public IntegrityHashConverter(string saltName, IHashingService hashingService)
        : base(v => ToDatabase(v, saltName, 1, hashingService), v => FromDatabase(v, saltName, hashingService)) { }

    public static string ToDatabase(string value, string keyName, int keyIndex, IHashingService hashingService)
    {
        var hashed = hashingService.CreateSaltedHash(value, keyName, keyIndex, HashingService.HashAlgorithm.SHA3_512);
        return $"{value}|{hashed}";
    }

    public static string FromDatabase(string value, string keyName, IHashingService hashingService)
    {
        var original = value.Substring(0, value.LastIndexOf("|"));
        var hash = value.Substring(value.LastIndexOf("|") + 1);

        if (hashingService.MatchesHash(original, hash, keyName))
            return original;
        else
        {
            //TODO: Log this
            return "ERROR";
        }
    }
}
