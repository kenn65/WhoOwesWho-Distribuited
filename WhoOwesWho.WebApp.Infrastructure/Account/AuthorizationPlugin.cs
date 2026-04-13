using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.Infrastructure.Settings;
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Account
{
    public class AuthorizationPlugin(IConfiguration configuration) : ApiPluginClientBase(configuration), IAuthorize
    {
        private readonly AppSettings appSettings = new(configuration);
       
        public async Task<AuthenticationResponseModel> AuthenticateAsync(AuthenticationRequestModel request)
        {
            var baseAddress = appSettings.AuthorizationMicroserviceBaseAddress;
            var apiKey = appSettings.AuthorizationMicroserviceApiKey;
            var endpoint = $"{baseAddress}/authenticate";
            return await PostAsync<AuthenticationResponseModel, AuthenticationRequestModel>(endpoint, request, apiKey!, true);
        }

        public async Task<AuthorizationResponseModel> AuthorizeAsync(AuthorizationRequestModel request)
        {
            var baseAddress = appSettings.AuthorizationMicroserviceBaseAddress;
            var apiKey = appSettings.AuthorizationMicroserviceApiKey;
            var endpoint = $"{baseAddress}/authorize";
            return await PostAsync<AuthorizationResponseModel, AuthorizationRequestModel>(endpoint, request, apiKey!, true);
        }
    }
}

