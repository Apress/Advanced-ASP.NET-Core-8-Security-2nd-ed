namespace JuiceShopDotNet.Common.Cryptography.Hashing;

public interface IHashingService
{
    string CreateUnsaltedHash(string plainText, HashingService.HashAlgorithm algorithm);
    string CreateSaltedHash(string plainText, string saltNameInKeyStore, int keyIndex, HashingService.HashAlgorithm algorithm);
    bool MatchesHash(string plainText, string hash, string saltNameInKeyStore);
}
