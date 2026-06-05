using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Extensions;
using WhoOwesWho.WebApp.Infrastructure.Settings;    
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Account
{
    public class AuthorizationPlugin : ApiPluginClientBase, IAuthorizationPlugin
    {
        private readonly AppSettings appSettings;
        private readonly ITokenService tokenService;
       
        public AuthorizationPlugin(IConfiguration configuration, ITokenService tokenService, NavigationManager nav)
            : base(configuration, tokenService, nav)
        {
            this.tokenService = tokenService;
            this.appSettings = new AppSettings(configuration);
        }

        public async Task<AuthenticationResponseModel> AuthenticateAsync(AuthenticationRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("authenticate");
            return await PostAsync<AuthenticationResponseModel, AuthenticationRequestModel>(endpoint, request, apiKey!, true);
        }

        public async Task<AuthorizationResponseModel> AuthorizeAsync(AuthorizationRequestModel request)
        {
            var apiKey = await GetApiKeyAsync();
            var baseAddress = await GetBaseAddressAsync();
            var endpoint = await baseAddress.ToEndpointAsync("authorize");
            return await PostAsync<AuthorizationResponseModel, AuthorizationRequestModel>(endpoint, request, apiKey!, true);
        }

        public async Task<CookiesResponseModel> RefreshAsync()
        {
            return await tokenService.RefreshAsync();
        }

        public async Task<CookiesDeletionResponseModel> DeleteCookiesAsync()
        {
            return await tokenService.DeleteCookiesAsync();
        }

        private async Task<string> GetBaseAddressAsync() => appSettings.AuthorizationMicroserviceBaseAddress!;
        private async Task<string> GetApiKeyAsync() => appSettings.AuthorizationMicroserviceApiKey!;
    }
}

