namespace JuiceShopDotNet.Common.Cryptography.SymmetricEncryption;

public interface IEncryptionService
{
    string Encrypt(string toEncrypt, string encryptionKeyName, int keyIndex);
    string Encrypt(string toEncrypt, string encryptionKeyName, int keyIndex, EncryptionService.EncryptionAlgorithm algorithm);
    string Decrypt(string toDecrypt, string encryptionKeyName);
}
