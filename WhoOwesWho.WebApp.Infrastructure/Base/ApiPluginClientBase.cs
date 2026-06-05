using Flurl.Http;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Base
{
    public abstract class ApiPluginClientBase(IConfiguration configuration, ITokenService tokenService, NavigationManager nav) : ApiPluginBase(configuration)
    {
        protected async Task<T> GetAsync<T>(string baseEndpoint, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, bool applyToken = false) where T : ResponseModelBase, new()
        {
            return await ExecuteWithRetryAsync<T>(applyToken, async requestToken =>
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, requestToken?.TokenValue);
                return await client.Request().GetJsonAsync<T>();
            });
        }

        protected async Task<T> PostAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, bool applyToken = false) where T : ResponseModelBase, new() where TR : RequestModelBase
        {
            return await ExecuteWithRetryAsync<T>(applyToken, async requestToken =>
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, requestToken?.TokenValue);
                var response = await client.Request().PostJsonAsync(request);

                if (typeof(T) == typeof(IFlurlResponse))
                    return (T)response;

                var result = await response.GetJsonAsync<T>();

                if (result is AuthorizationResponseModel authorizationResponse && authorizationResponse.Success)
                {
                    var cookies = authorizationResponse.Adapt<CookiesResponseModel>();
                    await tokenService.SetCookiesAsync(cookies);
                }

                return result;
            });
        }

        protected async Task<T> PutAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, bool applyToken = false) where T : ResponseModelBase, new() where TR : RequestModelBase
        {
            return await ExecuteWithRetryAsync<T>(applyToken, async requestToken =>
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, requestToken?.TokenValue);
                var response = await client.Request().PutJsonAsync(request);

                if (typeof(T) == typeof(IFlurlResponse))
                    return (T)response;

                return await response.GetJsonAsync<T>();
            });
        }

        protected async Task<T> PatchAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, bool applyToken = false) where T : ResponseModelBase, new() where TR : RequestModelBase
        {
            return await ExecuteWithRetryAsync<T>(applyToken, async requestToken =>
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, requestToken?.TokenValue);
                var response = await client.Request().PatchJsonAsync(request);

                if (typeof(T) == typeof(IFlurlResponse))
                    return (T)response;

                return await response.GetJsonAsync<T>();
            });
        }

        protected async Task<T> DeleteAsync<T>(string baseEndpoint, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, bool applyToken = false) where T : ResponseModelBase, new()
        {
            return await ExecuteWithRetryAsync<T>(applyToken, async requestToken =>
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, requestToken?.TokenValue);
                var response = await client.Request().DeleteAsync();

                if (typeof(T) == typeof(IFlurlResponse))
                    return (T)response;

                return await response.GetJsonAsync<T>();
            });
        }

        private async Task<T> ExecuteWithRetryAsync<T>(bool applyToken, Func<CookiesResponseModel?, Task<T>> execute) where T : ResponseModelBase, new()
        {
            try
            {
                var requestToken = applyToken ? await tokenService.GetAsync() : null;
                return await execute(requestToken);
            }
            catch (FlurlHttpException e) when (e.StatusCode == 401 && applyToken)
            {
                var refreshed = await tokenService.RefreshAsync();

                if (!refreshed.Success)
                {
                    return new T
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "Unauthorized"
                    };
                }

                try
                {
                    var newToken = await tokenService.GetAsync();
                    return await execute(newToken);
                }
                catch (FlurlHttpException retryException)
                {
                    return await HandleFlurlException<T>(retryException);
                }
            }
            catch (FlurlHttpException e)
            {
                return await HandleFlurlException<T>(e);
            }
        }

        private async Task<T> HandleFlurlException<T>(FlurlHttpException e) where T : ResponseModelBase, new()
        {
            try
            {
                if (e.StatusCode != 500)
                {
                    var response = await e.GetResponseJsonAsync<T>();
                    if (response is not null)
                        return response;
                }
                if (e.StatusCode == 500)
                {
                    nav.NavigateTo("/error/500", true);
                }
            }
            catch { }

            return new T
            {
                Success = false,
                StatusCode = e.StatusCode ?? 500,
                Message = e.Message
            };
        }
    }
}