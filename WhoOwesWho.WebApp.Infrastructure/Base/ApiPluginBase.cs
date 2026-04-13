using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Web;

namespace WhoOwesWho.WebApp.Infrastructure.Base
{
    public abstract class ApiPluginBase(IConfiguration configuration)
    {
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
            client.Headers.Add(configuration["Security:ApiKeyHeaderName"], apiKey);

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.Headers.Add("Authorization", $"Bearer {token}");
            }
        }

        protected static async Task<string> BuildEndpoint(string baseEndpoint, bool encode, IDictionary<string, dynamic>? parameters)
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
