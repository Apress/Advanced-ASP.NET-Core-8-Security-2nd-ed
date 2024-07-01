using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using NuGet.Protocol.Core.Types;
using System.Data;
using System.Security.Claims;

namespace JuiceShopDotNet.Safe.Logging;

public class SecurityLogger<T> : ISecurityLogger
{
    private IConfiguration _thisConfiguration { get; set; }
    private IHttpContextAccessor _httpContext { get; set; }
    private ILogger<T> _debugLogger { get; set; }

    public SecurityLogger(IConfiguration configuration, IHttpContextAccessor contextAccessor, ILogger<T> logger)
    {
        _thisConfiguration = configuration;
        _httpContext = contextAccessor;
        _debugLogger = logger;
    }

    public void Log(SecurityEventType securityEventType, string message)
    {
        Log(securityEventType.EventId, securityEventType.LogLevel, securityEventType.SecurityLogLevel, message, null);
    }

    public void Log(SecurityEventType securityEventType, string message, string? userID)
    {
        Log(securityEventType.EventId, securityEventType.LogLevel, securityEventType.SecurityLogLevel, message, userID);
    }

    public void Log(int eventId, LogLevel logLevel, SecurityEventType.SecurityLevel securityLevel, string message)
    {
        Log(eventId, logLevel, securityLevel, message, null);
    }

    public void Log(int eventId, LogLevel logLevel, SecurityEventType.SecurityLevel securityLevel, string message, string? userID)
    {
        string? ipAddress = null;
        int? port = null;
        string? path = null;
        string? query = null;
        string? userAgent = null;

        var context = _httpContext.HttpContext;

        if (context != null)
        {
            if (context.Connection != null)
            {
                ipAddress = context.Connection.RemoteIpAddress.ToString();
                port = context.Connection.RemotePort;
            }

            if (context.Request != null)
            {
                path = context.Request.Path;

                if (context.Request.QueryString != null)
                    query = context.Request.QueryString.ToString();

                if (context.Request.Headers.UserAgent.Any())
                    userAgent = context.Request.Headers.UserAgent.ToString();
            }
        }

        if (string.IsNullOrEmpty(userID))
        {
            var idClaim = _httpContext.HttpContext.User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (idClaim != null)
                userID = idClaim.Value;
        }

        using (var connection = new SqlConnection(_thisConfiguration.GetConnectionString("DefaultConnection")))
        {
            var command = connection.CreateCommand();
            command.CommandText = "INSERT SecurityEvent (SecurityLevelID, EventID, LoggedInUser, RequestIP, RequestPort, RequestUserAgent, QueryString, AdditionalInfo) VALUES (@SecurityLevelID, @EventID, @LoggedInUser, @RequestIP, @RequestPort, @RequestUserAgent, @QueryString, @AdditionalInfo)";

            command.Parameters.AddWithValue("@EventID", eventId);
            command.Parameters.AddWithValue("@SecurityLevelID", (int)securityLevel);

            command.Parameters.AddWithValue("@LoggedInUser", userID.ToDbNullable());

            command.Parameters.AddWithValue("@RequestIP", ipAddress.ToDbNullable());
            command.Parameters.AddWithValue("@RequestPort", port.ToDbNullable());
            command.Parameters.AddWithValue("@AdditionalInfo", message.ToDbNullable());
            command.Parameters.AddWithValue("@RequestUserAgent", userAgent.ToDbNullable());
            command.Parameters.AddWithValue("@RequestPath", path.ToDbNullable());
            command.Parameters.AddWithValue("@QueryString", query.ToDbNullable());

            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _debugLogger.LogError("An error occurred while checking if the user is locked out: " + ex.ToString());
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        if (_debugLogger != null)
            _debugLogger.Log(logLevel, message);
    }
}

public static class CommandExtensions
{
    public static object ToDbNullable(this string? value)
    {
        if (value == null)
            return DBNull.Value;
        else
            return value;
    }

    public static object ToDbNullable(this int? value)
    {
        if (value == null)
            return DBNull.Value;
        else
            return value;
    }
}
