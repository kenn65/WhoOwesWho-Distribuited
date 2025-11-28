using Flurl.Http;
using WhoOwesWho.AuthorizationService.Settings;

namespace WhoOwesWho.AuthorizationService.Services.Base
{
    public abstract class ServiceBase(IConfiguration configuration) 
    {
        private readonly AppSettings _settings = new(configuration);
        protected AppSettings AppSettings => _settings;

        protected IFlurlClient GetClient(string endpoint, string apiKey, string? token = null)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain,
                errors) => true;
            var httpClient = new HttpClient(httpClientHandler);
            var client = new FlurlClient(httpClient, endpoint);
            AddHeaders(client, apiKey, token);
            return client;
        }

        protected void AddHeaders(IFlurlClient client, string apiKey, string? token = null)
        {
            client.Headers.Add("Content-Type", "application/json");
            client.Headers.Add(AppSettings.ApiKeyHeaderName, apiKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }
            client.Headers.Add("Authorization", $"Bearer {token}");
        }
    }
}
