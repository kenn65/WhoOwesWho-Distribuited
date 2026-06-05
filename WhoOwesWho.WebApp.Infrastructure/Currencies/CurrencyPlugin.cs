using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Extensions;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Currencies.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Currencies
{
    public class CurrencyPlugin(IConfiguration configuration, ITokenService tokenService, NavigationManager nav) : ApiPluginClientBase(configuration, tokenService, nav), ICurrencyPlugin
    {
        private readonly AppSettings appSettings = new(configuration);
                
        public async Task<EnumerableWrapperResponseModel<IEnumerable<CurrencyResponseModel>>> GetCurrenciesAsync()
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync(string.Empty);
            return await GetAsync<EnumerableWrapperResponseModel<IEnumerable<CurrencyResponseModel>>>(endpoint, apiKey, true, applyToken: true);
        }

        private async Task<string> GetBaseAddressAsync() => appSettings.CurrencyMicroserviceBaseAddress!;
        private async Task<string> GetApiKeyAsync() => appSettings.CurrencyMicroserviceApiKey!;
    }
}
