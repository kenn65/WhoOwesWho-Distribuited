using Flurl.Http;
using Flurl.Http.Configuration;
using WhoOwesWho.MessagingService.Settings;

namespace WhoOwesWho.MessagingService.Services.Base
{
    public abstract class ServiceBase(IConfiguration configuration)
    {
        private readonly AppSettings _settings = new(configuration);
        protected AppSettings AppSettings => _settings;

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
            client.Headers.Add(AppSettings.ApiKeyHeaderName, apiKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }
            client.Headers.Add("Authorization", $"Bearer {token}");
        }
    }
}
