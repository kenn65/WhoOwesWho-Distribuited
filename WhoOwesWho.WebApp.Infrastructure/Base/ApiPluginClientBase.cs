using Flurl.Http;
using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.Infrastructure.Base
{
    public abstract class ApiPluginClientBase(IConfiguration configuration) : ApiPluginBase(configuration)
    {
        protected async Task<T> GetAsync<T>(string baseEndpoint, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : class
        {
            try
            {
                var endpoint = await BuildEndpoint(baseEndpoint, encode, parameters);
                using var client = GetClient(endpoint, apiKey, token);
                var response = await client.Request().GetJsonAsync<T>();
                return response;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        protected async Task<T> PostAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : ResponseModelBase where TR : RequestModelBase
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
                
        protected async Task<T> PutAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : ResponseModelBase where TR : RequestModelBase
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

        protected async Task<T> PatchAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : ResponseModelBase where TR : RequestModelBase
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

        protected async Task<T> DeleteAsync<T>(string baseEndpoint, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : ResponseModelBase
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
    }
}
