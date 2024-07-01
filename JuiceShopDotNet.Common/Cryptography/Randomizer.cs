using JuiceShopDotNet.Common.Cryptography.SymmetricEncryption;
using System.Security.Cryptography;

namespace JuiceShopDotNet.Common.Cryptography;

public static class Randomizer
{
    public static string CreateIV(EncryptionService.EncryptionAlgorithm algorithm)
    {
        var length = EncryptionService.GetIVLengthForAlgorithm(algorithm);
        return CreateRandomString(length);
    }

    public static string CreateRandomString(int length)
    {
        byte[] buffer = new byte[length];
        RandomNumberGenerator.Fill(buffer);
        return BitConverter.ToString(buffer).Replace("-", "");
    }

    public static byte[] CreateRandomByteArray(int length)
    {
        byte[] buffer = new byte[length];
        RandomNumberGenerator.Fill(buffer);
        return buffer;
    }
}
