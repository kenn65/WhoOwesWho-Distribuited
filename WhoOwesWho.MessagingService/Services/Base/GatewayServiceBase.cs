using System.Text;
using System.Web;
using Flurl.Http;

namespace WhoOwesWho.MessagingService.Services.Base
{
    public abstract class GatewayServiceBase(IConfiguration configuration) : ServiceBase(configuration)
    {
        public async Task<T> Get<T>(string baseEndpoint, string apiKey, IDictionary<string, dynamic> parameters)
        {
            var endpoint = await BuildEndpoint(baseEndpoint, parameters);
            using var client = GetClient(endpoint, apiKey);
            return await client.Request().GetJsonAsync<T>();
        }

        public async Task<T> Post<T, TR>(string baseEndpoint, TR request, string apiKey, IDictionary<string, dynamic>? parameters = null) where T : class where TR : class
        {
            var endpoint = await BuildEndpoint(baseEndpoint, parameters);
            using var client = GetClient(endpoint, apiKey);
            var response = await client.Request().PostJsonAsync(request);
            if (typeof(T) == typeof(IFlurlResponse))
            {
                return await Task.FromResult((T)response);
            }
            return await response.GetJsonAsync<T>();
        }

        public async Task<string> BuildEndpoint(string baseEndpoint, IDictionary<string, dynamic>? parameters)
        {
            if (parameters == null)
            {
                return baseEndpoint;
            }
            var endpointBuilder = new StringBuilder();
            endpointBuilder.Append(baseEndpoint);
            foreach (var parameter in parameters)
            {
                endpointBuilder.Append(endpointBuilder.ToString().Contains("?") ? "&" : "?");
                endpointBuilder.Append(parameter.Key);
                endpointBuilder.Append("=");
                endpointBuilder.Append(baseEndpoint.Contains("/protect")
                    ? HttpUtility.UrlEncode(parameter.Value)
                    : parameter.Value);
            }
            var defaultValue = endpointBuilder.ToString();
            var bytes = Encoding.Default.GetBytes(defaultValue);
            return await Task.FromResult(Encoding.UTF8.GetString(bytes));
        }
    }
}
