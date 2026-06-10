using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.Gateways;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthorizationSecurityService
    {
        public Task<string> ProtectAsync(string value, bool force = false);
        public Task<string> UnprotectAsync(string value, bool force = false);

        

        Task<bool> ValidateApiKey(string userApiKey);
    }

    public class AuthorizationSecurityService(IConfiguration configuration, IEncryptionGatewayService encryptionGatewayService) : ServiceBase(configuration), IAuthorizationSecurityService
    {
        public async Task<string> ProtectAsync(string value, bool force = false)
        {
            if (value == string.Empty)
            {
                return value;
            }

            if (value is null)
            {
                throw new Exception("Security service has null value entered for protection");
            }

            if (value.IsValid() || value.IsGuid() || force)
            {
                return await encryptionGatewayService.ProtectAsync(value, true);
            }
            return value;

        }

        public async Task<string> UnprotectAsync(string value, bool force = false)
        {
            if (value == string.Empty)
            {
                return value;
            }
            if (value is null)
            {
                throw new Exception("Security service has null value entered for unprotection");
            }

            if (!value.IsValid() && !value.IsGuid() || force)
            {
                return await encryptionGatewayService.UnprotectAsync(value, true);
            }
            return value;
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
