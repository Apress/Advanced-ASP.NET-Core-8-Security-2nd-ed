namespace JuiceShopDotNet.Safe.Logging;

public class SecurityLoggerFactory : ISecurityLoggerFactory
{
    private readonly IConfiguration _thisConfiguration;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILoggerFactory _loggerFactory;

    public SecurityLoggerFactory(IConfiguration configuration, IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory)
    {
        _thisConfiguration = configuration;
        _httpContext = contextAccessor;
        _loggerFactory = loggerFactory;
    }

    public ISecurityLogger CreateLogger<T>()
    {
        var logger = _loggerFactory.CreateLogger<T>();
        return new SecurityLogger<T>(_thisConfiguration, _httpContext, logger);
    }
}
