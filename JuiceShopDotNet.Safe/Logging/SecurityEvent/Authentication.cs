namespace JuiceShopDotNet.Safe.Logging;

public static partial class SecurityEvent
{
    public static class Authentication
    {
        public static SecurityEventType LOGIN_SUCCESSFUL { get; } = new SecurityEventType(1200, LogLevel.Information, SecurityEventType.SecurityLevel.SECURITY_SUCCESS);
        public static SecurityEventType LOGOUT_SUCCESSFUL { get; } = new SecurityEventType(1201, LogLevel.Information, SecurityEventType.SecurityLevel.SECURITY_SUCCESS);
        public static SecurityEventType PASSWORD_MISMATCH { get; } = new SecurityEventType(1202, LogLevel.Debug, SecurityEventType.SecurityLevel.SECURITY_INFO);
        public static SecurityEventType USER_LOCKED_OUT { get; } = new SecurityEventType(1203, LogLevel.Debug, SecurityEventType.SecurityLevel.SECURITY_WARNING);
        public static SecurityEventType USER_NOT_FOUND { get; } = new SecurityEventType(1204, LogLevel.Information, SecurityEventType.SecurityLevel.SECURITY_WARNING);
        public static SecurityEventType LOGIN_SUCCESS_2FA_REQUIRED { get; } = new SecurityEventType(1210, LogLevel.Information, SecurityEventType.SecurityLevel.SECURITY_INFO);
    }
}