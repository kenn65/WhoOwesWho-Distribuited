using Flurl.Http;
using WhoOwesWho.DatabaseMigrationService.Settings;

namespace WhoOwesWho.DatabaseMigrationService.Services.Base
{
    public abstract class ServiceBase(IConfiguration configuration)
    {
        private readonly AppSettings _settings = new(configuration);
        protected AppSettings AppSettings => _settings;

        protected IFlurlClient GetClient(string endpoint, string apiKey, string? token = null)
        {
            var client = new FlurlClient(endpoint);
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
