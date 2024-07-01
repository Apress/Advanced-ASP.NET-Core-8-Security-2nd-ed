namespace JuiceShopDotNet.Safe.Logging;

public class SecurityEventType
{
    public enum SecurityLevel
    {
        SECURITY_NA = 1,
        SECURITY_SUCCESS = 2,
        SECURITY_AUDIT = 3,
        SECURITY_INFO = 4,
        SECURITY_WARNING = 5,
        SECURITY_ERROR = 6,
        SECURITY_CRITICAL = 7
    }

    public int EventId { get; private set; }
    public SecurityLevel SecurityLogLevel { get; private set; }
    public LogLevel LogLevel { get; private set; }

    public SecurityEventType(int eventId, LogLevel logLevel, SecurityLevel securityLevel)
    {
        EventId = eventId;
        LogLevel = logLevel;
        SecurityLogLevel = securityLevel;
    }
}
