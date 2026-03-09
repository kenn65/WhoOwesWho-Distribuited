using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.Gateways;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthorizationSecurityService
    {
        public Task<string> ProtectAsync(string value);
        public Task<string> UnprotectAsync(string value);

        Task<AuthorizationResponseModel> ProtectCookiesAsync(UserMessageResponseModel user, string token, bool encode);

        Task<bool> ValidateApiKey(string userApiKey);
    }

    public class AuthorizationSecurityService(IConfiguration configuration, IEncryptionGatewayService encryptionGatewayService) : ServiceBase(configuration), IAuthorizationSecurityService
    {
        public async Task<string> ProtectAsync(string value)
        {
            return await encryptionGatewayService.ProtectAsync(value, true);
        }
                
        public async Task<string> UnprotectAsync(string value)
        {
            return await encryptionGatewayService.UnprotectAsync(value, true);
        }

        public async Task<AuthorizationResponseModel> ProtectCookiesAsync(UserMessageResponseModel user, string token, bool encode)
        {
            return await encryptionGatewayService.ProtectCookiesAsync(user, token, encode);
        }

        public async Task<bool> ValidateApiKey(string authorizationApiKey)
        {
            if (string.IsNullOrWhiteSpace(authorizationApiKey))
            {
                return false;
            }
            var apiKey = AppSettings.ApiKey;
            return await Task.FromResult(apiKey == authorizationApiKey);
        }
    }
}
