namespace JuiceShopDotNet.Safe.Data.EncryptedDataStore;

public interface IRemoteSensitiveDataStore
{
    EncryptedCreditApplication GetCreditApplication(int id);
    bool SaveCreditApplication(EncryptedCreditApplication application);

    EncryptedJuiceShopUser GetJuiceShopUser(int id);
    bool SaveJuiceShopUser(EncryptedJuiceShopUser user);
}
