using Flurl.Http;
using Flurl.Http.Configuration;
using WhoOwesWho.UserService.Settings;

namespace WhoOwesWho.UserService.Services.Base
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
            client.WithHeader("Content-Type", "application/json");
            client.WithHeader("Accept", "*/*");
            client.Headers.Add(AppSettings.ApiKeyHeaderName, apiKey);   

            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }
            client.Headers.Add("Authorization", $"Bearer {token}");
        }
    }
}
