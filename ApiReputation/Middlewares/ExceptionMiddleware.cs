using Serilog;
using System.Net;
using System.Text.Json;

namespace ApiReputation.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error("🔥 Exception: {Message} | Path: {Path} | User: {User}",
                    ex.Message, context.Request.Path, context.User.Identity.Name);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                message = "Sunucuda bir hata oluştu.",
                details = exception.Message
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
