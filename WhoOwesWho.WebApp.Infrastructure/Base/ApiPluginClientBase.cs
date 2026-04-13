using Flurl.Http;
using Microsoft.Extensions.Configuration;

namespace WhoOwesWho.WebApp.Infrastructure.Base
{
    public abstract class ApiPluginClientBase(IConfiguration configuration) : ApiPluginBase(configuration)
    {
        public async Task<T> GetAsync<T>(string baseEndpoint, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null)
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

        public async Task<T> PostAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : class where TR : class
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

        public async Task<T> PutAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : class where TR : class
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

        public async Task<T> PatchAsync<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : class where TR : class
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
    }

}
