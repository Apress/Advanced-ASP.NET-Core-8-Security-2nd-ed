namespace JuiceShopDotNet.Common.Cryptography.AsymmetricEncryption;

public class KeyPair
{
    /// <summary>
    /// This public key can be public
    /// </summary>
    public string PublicKey { get; set; }

    /// <summary>
    /// Keep the private key PRIVATE - if this is made public, you should regenerate a new set of keys
    /// </summary>
    public string PrivateKey { get; set; }
}
