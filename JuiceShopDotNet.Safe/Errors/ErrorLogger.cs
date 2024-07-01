using JuiceShopDotNet.Safe.Logging;
using Microsoft.AspNetCore.Diagnostics;

namespace JuiceShopDotNet.Safe.Errors;

public class ErrorLogger : IExceptionHandler
{
    //Valuable if you want the error handler to change the UI
    //Use env.IsProduction() to determine which error message to show
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ISecurityLogger _securityLogger;
    
    public ErrorLogger(IHostEnvironment env, ISecurityLoggerFactory loggerFactory)
    { 
        _hostEnvironment = env;
        _securityLogger = loggerFactory.CreateLogger<ErrorLogger>();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _securityLogger.Log(SecurityEvent.General.EXCEPTION, exception.ToString());

        httpContext.Response.ContentType = "application/json";
        var responseObject = new { message = "An unknown error occurred" };
        var responseJson = System.Text.Json.JsonSerializer.Serialize(responseObject);
        await httpContext.Response.WriteAsync(responseJson, cancellationToken);
        return true;

        ////Use this to redirect to the error page we set in Program.cs
        //return ValueTask.FromResult(false);
    }
}
