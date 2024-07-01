using static JuiceShopDotNet.Safe.Logging.SecurityEventType;

namespace JuiceShopDotNet.Safe.Logging;

public interface ISecurityLogger
{
    void Log(SecurityEventType securityEventType, string message);
    void Log(int eventId, LogLevel logLevel, SecurityLevel securityLevel, string message);
    void Log(SecurityEventType securityEventType, string message, string? overrideUserID);
    void Log(int eventId, LogLevel logLevel, SecurityLevel securityLevel, string message, string? overrideUserID);
}
