using Microsoft.AspNetCore.Http;
using Serilog;
using System.Diagnostics;
using System.Threading.Tasks;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var user = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "Anonymous";
            Log.Information("📢 Request: {Method} {Path} | User: {User} | Time: {Time}",
                context.Request.Method, context.Request.Path, user, DateTime.UtcNow);

            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error("❌ Error: {Message} | Path: {Path} | User: {User}",
                ex.Message, context.Request.Path, context.User.Identity.Name);

            throw;
        }
        finally
        {
            stopwatch.Stop();
            Log.Information("✅ Response: {StatusCode} | Path: {Path} | TimeTaken: {Elapsed} ms",
                context.Response.StatusCode, context.Request.Path, stopwatch.ElapsedMilliseconds);
        }
    }
}