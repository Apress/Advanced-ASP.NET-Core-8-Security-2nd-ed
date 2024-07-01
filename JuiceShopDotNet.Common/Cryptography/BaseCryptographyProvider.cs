using System.Text;

namespace JuiceShopDotNet.Common.Cryptography;

public abstract class BaseCryptographyProvider
{
    protected CipherTextInfo BreakdownCipherText(string cipherText)
    {
        var info = new CipherTextInfo();

        if (cipherText.Length > 5 && cipherText[0] == '[')
        {
            var algorithmIndexPair = cipherText.Substring(1, cipherText.IndexOf(']') - 1).Split(",");

            info.Algorithm = int.Parse(algorithmIndexPair[0]);

            if (algorithmIndexPair.Length > 1)
                info.Index = int.Parse(algorithmIndexPair[1]);
            else
                info.Index = null;

            info.CipherText = cipherText.Substring(cipherText.IndexOf(']') + 1);
        }
        else
        {
            info.Algorithm = null;
            info.Index = null;
            info.CipherText = cipherText;
        }

        return info;
    }

    //internal CipherTextInfo BreakdownCipherTextAndSalt(string cipherText)
    //{
    //    var info = BreakdownCipherText(cipherText);
    //    var saltLength = HashingService.GetSaltLength((HashingService.HashAlgorithm)info.Algorithm.Value);

    //    if (info.Algorithm.HasValue && info.CipherText.Length > saltLength)
    //    {
    //        info.Salt = info.CipherText.Substring(0, saltLength * 2);
    //        info.CipherText = info.CipherText.Substring(saltLength * 2);
    //    }

    //    return info;
    //}

    protected static byte[] HexStringToByteArray(string stringInHexFormat)
    {
        return Enumerable.Range(0, stringInHexFormat.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(stringInHexFormat.Substring(x, 2), 16))
                         .ToArray();
    }

    protected static string ByteArrayToString(byte[] bytes)
    {
        var sb = new StringBuilder();
        foreach (var b in bytes)
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }

    protected struct CipherTextInfo
    {
        public int? Algorithm { get; set; }
        public int? Index { get; set; }
        public string CipherText { get; set; }
        public string Salt { get; set; }
    }
}
