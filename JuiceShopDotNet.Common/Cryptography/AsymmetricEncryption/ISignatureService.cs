namespace JuiceShopDotNet.Common.Cryptography.AsymmetricEncryption;

public interface ISignatureService
{
    /// <summary>
    /// Decrypts a string
    /// </summary>
    /// <param name="textToSign">Text to sign</param>
    /// <param name="keyInXMLFormat">Key in XML Format</param>
    /// <returns>Decrypted string</returns>
    string CreateSignature(string textToSign, string keyNameInStore, int keyIndex, SignatureService.SignatureAlgorithm algorithm);

    /// <summary>
    /// Decrypts a string
    /// </summary>
    /// <param name="textToVerify">Plain text to verify</param>
    /// <param name="textToVerify">Old Signature</param>
    /// <param name="keyInXMLFormat">Key in XML Format</param>
    /// <returns>True if signature could be verified</returns>
    bool VerifySignature(string textToVerify, string oldSignature, string keyNameInStore);

    KeyPair GenerateKeys();
}
