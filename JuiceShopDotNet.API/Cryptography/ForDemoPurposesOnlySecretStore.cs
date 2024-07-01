using JuiceShopDotNet.Common.Cryptography.KeyStorage;

namespace JuiceShopDotNet.API.Cryptography;

public class ForDemoPurposesOnlySecretStore : ISecretStore
{
    public string GetKey(string keyName, int keyIndex)
    {
        switch (keyName) 
        {
            case KeyNames.ApiPublicKey:
                return "<RSAKeyValue><Modulus>q6tP3Fndiq5x7g1t3TomKBYwJKjJEb658NmHFkDok7m7ng4m2IwWZvMW60buxRdXxAoC10YOygbch+kIpqYDyozs2eUNccuQQq4XJnU9xPe5KXZpkfTr9/98/Fu4CTdpf7PAUl7uCLQqPWoKUCWvxtetNexsr/sz3iCCxBlarVMFlSGJjePyA4H+WU0Pv10ZyfH28gtyt+AU/Vz0cti00ciXN3qlFuEV052D7NLfRSIWi9QrZMrqnsJ7mLAIF3Xdeu7SJQuDixbJTxT5AwB6HN7xcwOZx5hGZINC3HT3QSy7b4LcuHb1R4hUORIgf+d8V2Zol0mRdX2VRUG3Ib5GkQ==</Modulus><Exponent>AQAB</Exponent><P></P><Q></Q><DP></DP><DQ></DQ><InverseQ></InverseQ><D></D></RSAKeyValue>";
            case KeyNames.CreditApplication_SocialSecurityNumber:
                return "CE45338B9E76542251D7D600950F9755E8A84AD5AEAA74A0ED29D2575893DB95";
            case KeyNames.JuiceShopUser_Username:
                return "78ABD1C32EE2197615DE03A815F7BDBDC14BFCBC696397C901296AC1754633A8";
            case KeyNames.JuiceShopUser_UserEmail:
                return "8378C1DED2E2F70C88BB5F3824C303D30790FD27173BDA42787F2015DCFD8139";
            case KeyNames.JuiceShopUser_NormalizedUserEmail:
                return "2B0A3C3AB1E8CFD7833CACD59C43607F59BBF5E92138E8B838FB6577C30C37F9";
            case KeyNames.JWTKey:
                return "55176E3488E3F9BF165A42BDF49064C1CFEF5791347336F052E850FAD8C94589";
            default:
                throw new NotImplementedException($"Cannot find key: {keyName}");
        }
    }
}

public static class KeyNames
{
    public const string ApiPublicKey = "API_PUBLIC_KEY";
    public const string CreditApplication_SocialSecurityNumber = "CreditApplication_SocialSecurityNumber";
    public const string JuiceShopUser_Username = "JuiceShopUser_Username";
    public const string JuiceShopUser_UserEmail = "JuiceShopUser_UserEmail";
    public const string JuiceShopUser_NormalizedUserEmail = "JuiceShopUser_NormalizedUserEmail";
    public const string JWTKey = "JWTKey";
}
