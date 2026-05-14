using Flurl.Http;
using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.Infrastructure.Base
{
    public abstract class ApiPluginClientBase(IConfiguration configuration) : ApiPluginBase(configuration)
    {
        protected async Task<T> GetAsync<T>(string baseEndpoint, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : ResponseModelBase, new()
        {
            try
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, token);
                var response = await client.Request().GetJsonAsync<T>();
                return response;
            }
            catch (FlurlHttpException e)
            {
                return await HandleFlurlException<T>(e);
            }
        }

        protected async Task<T> PostAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : ResponseModelBase, new() where TR : RequestModelBase
        {
            try
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, token);
                var response = await client.Request().PostJsonAsync(request);
                if (typeof(T) == typeof(IFlurlResponse))
                {
                    return (T)response;
                }
                return await response.GetJsonAsync<T>();
            }
            catch (FlurlHttpException e)
            {
                return await HandleFlurlException<T>(e);
            }
        }

        protected async Task<T> PutAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : ResponseModelBase, new() where TR : RequestModelBase
        {
            try
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, token);
                var response = await client.Request().PutJsonAsync(request);
                if (typeof(T) == typeof(IFlurlResponse))
                {
                    return (T)response;
                }
                return await response.GetJsonAsync<T>();
            }
            catch (FlurlHttpException e)
            {
                return await HandleFlurlException<T>(e);
            }
        }

        protected async Task<T> PatchAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : ResponseModelBase, new() where TR : RequestModelBase
        {
            try
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, token);
                var response = await client.Request().PatchJsonAsync(request);
                if (typeof(T) == typeof(IFlurlResponse))
                {
                    return (T)response;
                }
                return await response.GetJsonAsync<T>();
            }
            catch (FlurlHttpException e)
            {
                return await HandleFlurlException<T>(e);
            }
        }

        protected async Task<T> DeleteAsync<T>(string baseEndpoint, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : ResponseModelBase, new()
        {
            try
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, token);
                var response = await client.Request().DeleteAsync();
                if (typeof(T) == typeof(IFlurlResponse))
                {
                    return (T)response;
                }
                return await response.GetJsonAsync<T>();
            }
            catch (FlurlHttpException e)
            {
                return await HandleFlurlException<T>(e);
            }

        }

        private async Task<T> HandleFlurlException<T>(FlurlHttpException ex) where T : ResponseModelBase, new()
        {
            try
            {
                var response = await ex.GetResponseJsonAsync<T>();

                if (response is not null)
                {
                    return response;
                }
            }
            catch
            {
            }

            return new T
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
