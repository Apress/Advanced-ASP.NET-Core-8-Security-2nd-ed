namespace JuiceShopDotNet.Safe.CSP;

public class NonceContainer
{
    public string ID { get; private set; } = Guid.NewGuid().ToString().Substring(0, 8);
}
