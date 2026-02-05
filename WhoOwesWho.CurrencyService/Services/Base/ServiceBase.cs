using Flurl.Http;
using Flurl.Http.Configuration;
using WhoOwesWho.CurrencyService.Settings;

namespace WhoOwesWho.CurrencyService.Services.Base
{
    public abstract class ServiceBase(IConfiguration configuration)
    {
        private readonly AppSettings _settings = new(configuration);
        protected AppSettings AppSettings => _settings;

        protected IFlurlClient GetClient(string endpoint)
        { 
            var client = new FlurlClientBuilder(endpoint)
            .ConfigureInnerHandler(handler =>
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            })
            .Build();
            AddHeaders(client);
            return client;
        }

        protected void AddHeaders(IFlurlClient client)
        {
            client.Headers.Add("Content-Type", "application/json");
        }
    }
}
