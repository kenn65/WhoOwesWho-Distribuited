using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Protection;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Extensions;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Protection.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Protection
{
    public class ProtectionPlugin(IConfiguration configuration) : ApiPluginClientBase(configuration), IProtectionPlugin
    {
        private readonly AppSettings appSettings = new(configuration);

        public async Task<ProtectionResponseModel> ProtectAsync(string text)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetProtectionBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"protect/{text}");
            return await GetAsync<ProtectionResponseModel>(endpoint, apiKey!, true);
        }

        public async Task<ProtectionResponseModel> UnprotectAsync(string text)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetProtectionBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync($"unprotect/{text}");
            return await GetAsync<ProtectionResponseModel>(endpoint, apiKey!, true);
        }

        public async Task<ProtectionResponseModel> ProtectCookiesAsync(CookiesRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetProtectionBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("/cookies/protect");
            return await PostAsync<ProtectionResponseModel, CookiesRequestModel>(endpoint, request, apiKey!, true);
        }

        private async Task<string> GetProtectionBaseAddressAsync() => appSettings.EncryptionMicroserviceBaseAddress!;
        
        private async Task<string> GetApiKeyAsync() => appSettings.EncryptionMicroserviceApiKey!;
    }
}
