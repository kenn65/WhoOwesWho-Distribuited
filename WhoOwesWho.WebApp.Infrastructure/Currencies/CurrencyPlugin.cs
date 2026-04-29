using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Extensions;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Currencies.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Currencies
{
    public class CurrencyPlugin(IConfiguration configuration) : ApiPluginClientBase(configuration), ICurrencyPlugin
    {
        private readonly AppSettings appSettings = new(configuration);
                
        public async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync(string jwtToken)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync(string.Empty);
            return await GetAsync<IEnumerable<CurrencyResponseModel>>(endpoint, apiKey, true, null, jwtToken);
        }

        private async Task<string> GetBaseAddressAsync() => appSettings.CurrencyMicroserviceBaseAddress!;
        private async Task<string> GetApiKeyAsync() => appSettings.CurrencyMicroserviceApiKey!;
    }
}
