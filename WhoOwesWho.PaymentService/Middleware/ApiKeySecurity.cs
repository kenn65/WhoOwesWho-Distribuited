using System.Net;
using WhoOwesWho.PaymentService.Services.Base;

namespace WhoOwesWho.PaymentService.Middleware
{
    public class ApiKeySecurity(IConfiguration configuration, RequestDelegate next) : ServiceBase(configuration)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            if (string.IsNullOrWhiteSpace(context.Request.Headers[AppSettings.ApiKeyHeaderName!]))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            string? userApiKey = context.Request.Headers[AppSettings.ApiKeyHeaderName!];

            if (!await ValidateApiKey(userApiKey!))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            await next(context);
        }

        private async Task<bool> ValidateApiKey(string userApiKey)
        {
            if (string.IsNullOrWhiteSpace(userApiKey))
            {
                return false;
            }
            var apiKey = AppSettings.ApiKey;
            return apiKey == userApiKey;
        }

    }
}
