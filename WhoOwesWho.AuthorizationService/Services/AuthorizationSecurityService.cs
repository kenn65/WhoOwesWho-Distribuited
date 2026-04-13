using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.Gateways;
using WhoOwesWho.Shared.Extensions;
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
            if (value.IsValid() || value.IsGuid())
            {
                return await encryptionGatewayService.ProtectAsync(value, true);
            }
            return value;
        }

        public async Task<string> UnprotectAsync(string value)
        {
            if (!value.IsValid() && !value.IsGuid())
            {
                return await encryptionGatewayService.UnprotectAsync(value, true);
            }
            return value;
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
            return apiKey == authorizationApiKey;
        }
    }
}
