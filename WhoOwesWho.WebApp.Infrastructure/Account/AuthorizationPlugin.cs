using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Account
{
    public class AuthorizationPlugin(IConfiguration configuration) : ApiPluginClientBase(configuration), IAuthorizationPlugin
    {
        private readonly AppSettings appSettings = new(configuration);

        public async Task<AuthenticationResponseModel> AuthenticateAsync(AuthenticationRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var endpoint = await CreateEndpoint("authenticate");
            return await PostAsync<AuthenticationResponseModel, AuthenticationRequestModel>(endpoint, request, apiKey!, true);
        }

        public async Task<AuthorizationResponseModel> AuthorizeAsync(AuthorizationRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var endpoint = await CreateEndpoint("authorize");
            return await PostAsync<AuthorizationResponseModel, AuthorizationRequestModel>(endpoint, request, apiKey!, true);
        }
        
        private async Task<string> GetBaseAddressAsync() => appSettings.AuthorizationMicroserviceBaseAddress!;
        private async Task<string> GetApiKeyAsync() => appSettings.AuthorizationMicroserviceApiKey!;

        private async Task<string> CreateEndpoint(string trailingPath)
        {
            var baseAddress = await GetBaseAddressAsync();
            return $"{baseAddress}/{trailingPath}";
        }
    }
}

