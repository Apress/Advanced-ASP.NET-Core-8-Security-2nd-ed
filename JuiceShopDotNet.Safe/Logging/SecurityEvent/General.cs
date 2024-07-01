namespace JuiceShopDotNet.Safe.Logging;

public static partial class SecurityEvent
{ 
    public static class General
    {
        public static SecurityEventType EXCEPTION { get; } = new SecurityEventType(0, LogLevel.Error, SecurityEventType.SecurityLevel.SECURITY_WARNING);
    }
}
