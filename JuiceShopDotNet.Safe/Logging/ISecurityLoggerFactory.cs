namespace JuiceShopDotNet.Safe.Logging;

public interface ISecurityLoggerFactory
{
    public ISecurityLogger CreateLogger<T>();
}
