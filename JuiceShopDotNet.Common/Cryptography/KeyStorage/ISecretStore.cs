namespace JuiceShopDotNet.Common.Cryptography.KeyStorage;

public interface ISecretStore
{
    /// <summary>
    /// Gets the full key from the key store.
    /// </summary>
    /// <returns>String representation of the key</returns>
    string GetKey(string keyName, int keyIndex);
}
