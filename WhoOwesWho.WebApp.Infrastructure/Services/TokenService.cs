using Flurl.Http;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Base;

namespace WhoOwesWho.WebApp.Infrastructure.Services
{
    public class TokenService : TokenServiceBase, ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpContext HttpContext => _httpContextAccessor.HttpContext!;

        public TokenService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            NavigationManager nav,
            IJSRuntime js)
            : base(js, nav, configuration, httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SetCookiesAsync(CookiesResponseModel data)
        {
            if (!_httpContextAccessor.HttpContext?.Response.HasStarted == false)
            {
                // Initial request — set directly
                await AppendCookies(data);
            }
            else
            {
                // Interactive session — use JS interop to call controller
                await AppendCookiesAsync(data);
            }
        }

        public async Task<CookiesDeletionResponseModel> DeleteCookiesAsync()
        {
            await RemoveCookiesAsync();
            if (_httpContextAccessor.HttpContext is null)
            {
                return new CookiesDeletionResponseModel
                {
                    Success = false
                };
            }
            var cookies = await GetAsync();
            var request = new RefreshRequestModel
            {
                RefreshToken = cookies?.RefreshValue ?? string.Empty
            };
            
            var endpoint = $"{BaseUrl}/delete";
            using var client = GetClient(endpoint, ApiKey);
            var response = await client.Request().PostJsonAsync(request);
            var result = await response.GetJsonAsync<CookiesDeletionResponseModel>();
            if (!result.Success)
            {
                return new CookiesDeletionResponseModel();
            }
            return new CookiesDeletionResponseModel
            {
                Success = true
            };
        }

        public Task<CookiesResponseModel?> GetAsync()
        {
            var request = HttpContext.Request;
            var tokenName = new CookiesResponseModel();
            var jwt = request.Cookies[tokenName.TokenName];
            var refresh = request.Cookies[new CookiesResponseModel().RefreshName];

            return Task.FromResult<CookiesResponseModel?>(new CookiesResponseModel
            {
                TokenValue = jwt ?? string.Empty,
                RefreshValue = refresh ?? string.Empty
            });
        }

        public async Task<CookiesResponseModel> RefreshAsync()
        {
            try
            {
                if (_httpContextAccessor.HttpContext is null)
                {
                    return new CookiesResponseModel { Success = false };
                }
                var cookies = await GetAsync();
                if (string.IsNullOrWhiteSpace(cookies!.RefreshValue))
                {
                    return new CookiesResponseModel 
                    {
                        Success = false 
                    };
                }
                var request = new RefreshRequestModel
                {
                    RefreshToken = cookies?.RefreshValue ?? string.Empty
                };

                var endpoint = $"{BaseUrl}/refresh";
                using var client = GetClient(endpoint, ApiKey);
                var response = await client.Request().PostJsonAsync(request);
                var intermediate = await response.GetJsonAsync<AuthorizationResponseModel>();
                var result = intermediate.Adapt<CookiesResponseModel>();

                if (!result.Success)
                {
                    return new CookiesResponseModel();
                }

                var ctx = HttpContext;

                ctx.Response.Cookies.Append(result.TokenName, result.TokenValue, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddMinutes(10)
                });

                ctx.Response.Cookies.Append(result.RefreshName, result.RefreshValue, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(90)
                });

                return new CookiesResponseModel
                {
                    Success = true
                };
            }
            catch
            {
                return new CookiesResponseModel();
            }
        }
    }
}