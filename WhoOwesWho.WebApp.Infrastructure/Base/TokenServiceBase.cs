using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;

namespace WhoOwesWho.WebApp.Infrastructure.Base
{
    public abstract class TokenServiceBase(IJSRuntime js, NavigationManager nav, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        protected string BaseUrl => configuration["AuthorizationMicroservice:BaseAddress"]!;
        protected string ApiKey => configuration["AuthorizationMicroservice:Security:ApiKey"]!;
        private string baseUrl => nav.BaseUri;
        protected async Task AppendCookiesAsync(CookiesResponseModel data)
        {
            await js.InvokeVoidAsync(
                "cookieApi.setCookies",
                $"{baseUrl}api/auth/set-cookies",
                data
            );
        }

        protected async Task AppendRefreshedCookiesAsync(CookiesResponseModel data)
        {
            var ctx = httpContextAccessor.HttpContext;

            if (!string.IsNullOrWhiteSpace(data.TokenValue))
            {
                ctx.Response.Cookies.Append(data.TokenName, data.TokenValue, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddMinutes(10)
                });
            }

            if (!string.IsNullOrWhiteSpace(data.RefreshValue))
            {
                ctx.Response.Cookies.Append(data.RefreshName, data.RefreshValue, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(90)
                });
            }
        }


        protected async Task RemoveCookiesAsync()
        {
            await js.InvokeVoidAsync(
                "cookieApi.deleteCookies",
                $"{baseUrl}api/auth/delete-cookies"
            );
        }



        protected IFlurlClient GetClient(string endpoint, string apiKey, string? token = null)
        {
            var client = new FlurlClientBuilder(endpoint)
            .ConfigureInnerHandler(handler =>
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            })
            .Build();

            AddHeaders(client, apiKey, token);
            return client;
        }

        protected void AddHeaders(IFlurlClient client, string apiKey, string? token = null)
        {
            client.Headers.Add("Content-Type", "application/json");
            client.Headers.Add("Accept", "*/*");
            client.Headers.Add(configuration["Security:ApiKeyHeaderName"], apiKey);

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.Headers.Add("Authorization", $"Bearer {token}");
            }
        }

    }
}
