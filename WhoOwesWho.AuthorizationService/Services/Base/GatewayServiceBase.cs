using System.Text;
using System.Web;
using Flurl.Http;

namespace WhoOwesWho.AuthorizationService.Services.Base
{
    public abstract class GatewayServiceBase(IConfiguration configuration) : ServiceBase(configuration)
    {
        public async Task<T> Get<T>(string baseEndpoint, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null)
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

        public async Task<T> Post<T, TR>(string baseEndpoint, TR request, string apiKey, bool encode, IDictionary<string, dynamic>? parameters = null, string? token = null) where T : class where TR : class
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

        public async Task<string> BuildEndpoint(string baseEndpoint, bool encode, IDictionary<string, dynamic>? parameters)
        {
            if (parameters is null || !parameters.Any())
            {
                return baseEndpoint;
            }
            var endpointBuilder = new StringBuilder();
            endpointBuilder.Append(baseEndpoint);
            foreach (var parameter in parameters)
            {
                endpointBuilder.Append(endpointBuilder.ToString().Contains('?') ? "&" : "?");
                endpointBuilder.Append(parameter.Key);
                endpointBuilder.Append("=");
                if (parameter.Value is bool)
                {
                    endpointBuilder.Append(parameter.Value);
                }
                else
                {
                    endpointBuilder.Append(encode
                        ? HttpUtility.UrlEncode(parameter.Value)
                        : parameter.Value);
                }
            }
            return endpointBuilder.ToString();
        }
    }
}
