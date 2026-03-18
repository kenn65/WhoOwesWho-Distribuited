using System.Net;
using WhoOwesWho.EncryptionService.Services.Base;

namespace WhoOwesWho.EncryptionService.Middleware
{
    public class ApiKeyMiddleware(IConfiguration configuration, RequestDelegate next) : ServiceBase(configuration)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Request.Headers[AppSettings.ApiKeyHeaderName]))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            string? userApiKey = context.Request.Headers[AppSettings.ApiKeyHeaderName];

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
