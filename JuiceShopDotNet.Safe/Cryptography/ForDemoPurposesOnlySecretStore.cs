using JuiceShopDotNet.Common.Cryptography.KeyStorage;

namespace JuiceShopDotNet.Safe.Cryptography;

public class ForDemoPurposesOnlySecretStore : ISecretStore
{
    public string GetKey(string keyName, int keyIndex)
    {
        switch (keyName)
        {
            case KeyNames.ApiPrivateKey:
                return "<RSAKeyValue><Modulus>q6tP3Fndiq5x7g1t3TomKBYwJKjJEb658NmHFkDok7m7ng4m2IwWZvMW60buxRdXxAoC10YOygbch+kIpqYDyozs2eUNccuQQq4XJnU9xPe5KXZpkfTr9/98/Fu4CTdpf7PAUl7uCLQqPWoKUCWvxtetNexsr/sz3iCCxBlarVMFlSGJjePyA4H+WU0Pv10ZyfH28gtyt+AU/Vz0cti00ciXN3qlFuEV052D7NLfRSIWi9QrZMrqnsJ7mLAIF3Xdeu7SJQuDixbJTxT5AwB6HN7xcwOZx5hGZINC3HT3QSy7b4LcuHb1R4hUORIgf+d8V2Zol0mRdX2VRUG3Ib5GkQ==</Modulus><Exponent>AQAB</Exponent><P>4AuXs71F8jys3DZC3C/22eSPswlYJ7xyWIFCEg/fYb/Ae2qHodxz5ubH2k3oTBqG13VxHmjXqZ12AphsgTnmdhPGgHVkUZ1Z6Z6nkxIYgmFviH67unihUkHwmcCutrcLtIOtC6QgVDFQwLOnHWL3YpcBWqQjjfslu1qQrs0c0ts=</P><Q>xCdZEK3M7oxTr+1CAJ8VrSm2T3L8pjGCoGHxx4SuohON/Y862Ngf9pOyDrgtd/BDl3GWW2H2ZLY84X0cjiBarUgvRo76Odm1jgpMe4fzMK0zRLUp29Q4BUdHCJ7+nawiPNpcWXZUAs1cFoa4MtbnOOtQDk8B7PL6HkMhUqVFygM=</Q><DP>eGLiRgtrHUmrHLzvWj9Ppi9hY7OeseNZkeMKrIfo4S5W8DoC3V+Gy8iwFMaODu6mC/ooKU3urE5Wzfg3PYzuH/5qSDZMDGq/mH/OzYEIuG5ArxhiKUWOcZPLA+L8PmPHH10ty7aKRJMnMBSYtHqsMUawzJKsJCuST8TPP44pccc=</DP><DQ>BPcmBwkPRf6hY7Oy1wcv6klDBzHW+XIJZ3vzPeS68vlhv8hvaevWq0xD1qGM7RtU6rGCZ9/L9/KxCdg779Eb2oUYRUX7SZmcQfM6ymm/mzzXLmcTny/5FxEd2DcGJQGlgDra2ZzoNYXzTdKtOQQ9qDA0v5f6aYAhGsECR/BNGvc=</DQ><InverseQ>DT2Jbmf49JUuiaCaU9LCXBQrNVKk7PiBkkR0SYe10fRboz0V+a2gUJpdM93UGjg45e0wo2OBulCYP7nbg8rNpBNXp6yzZ8uTOn6PHCiGpGnMpdQJ15jhBCp/GNC4x3tNPHrm+45ObMlYza/v2lrJm8QkFtE/ex7oqc20nGyL5XI=</InverseQ><D>YcEYpcqKjDkP6VW+VPaS2crguU9fx/oKmerUsMhfaBegjb1TQ4ZqD0+nomxu9M55DMoCmiFrtIE5vS7m3Ta1+/ZJvT5gbIdVa5ME3cJvXSUPTFwAe7uzTzIuMunrn1vteGmcP4uNEmm9j+E9ZCxBrwILwSQTNBbgj0GGFTK23vDzel15NH1GyUEDDzskPpIzLCkb+Jf7uHtoMkFr6l8v8IkLhWvEvttoFeJfh7rxsURO01o4XA6QRzHmvilIBT3sKkz4mC/GyGjuhqJBuWiBnUqD/WkS5Cerjja/ea6YwL8gw9ieU9z701MhF0wtq2PM26msdQSTy5uFi0IbXHLe7Q==</D></RSAKeyValue>";
            case KeyNames.JuiceShopUser_UserName_Salt:
                return "7900E574B61528CC9A3A17CED60FB09FD72A7ADEDAA9B56C985ACA12FF6F8658";
            case KeyNames.JuiceShopUser_UserEmail_Salt:
                return "A5F54189C812B8C014D76D4F21F7EDA31B6EAFAADA6DA361D7958D0E1FBDA589";
            case KeyNames.JuiceShopUser_NormalizedUserEmail_Salt:
                return "C826A8D4FBC46F4F50A2C2BEDAA2951B0349C8448E9C5E5349E47BC2E12184D1";
            case KeyNames.ProductReview_ReviewText_Salt:
                return "AE05538CEF2BC49E411D43D1D9BE38877D455BED638BFC7A4179E5CA8AFA0302";
            case KeyNames.JWTKey:
                return "55176E3488E3F9BF165A42BDF49064C1CFEF5791347336F052E850FAD8C94589";
            default:
                throw new NotImplementedException($"Cannot find key: {keyName}");
        }
    }
}

public static class KeyNames
{
    public const string ApiPrivateKey = "API_PRIVATE_KEY";
    public const string JuiceShopUser_UserName_Salt = "JuiceShopUser_UserName_Salt";
    public const string JuiceShopUser_UserEmail_Salt = "JuiceShopUser_UserEmail_Salt";
    public const string JuiceShopUser_NormalizedUserEmail_Salt = "JuiceShopUser_NormalizedUserEmail_Salt";
    public const string ProductReview_ReviewText_Salt = "ProductReview_ReviewText_Salt";
    public const string JWTKey = "JWTKey";
}
