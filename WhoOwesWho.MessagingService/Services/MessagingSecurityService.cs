using WhoOwesWho.MessagingService.Services.Base;
using WhoOwesWho.MessagingService.Services.Gateways;
using WhoOwesWho.MessagingService.Settings;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.MessagingService.Services
{
    public interface IMessagingSecurityService
    {
        public Task<string> ProtectAsync(string value);
        public Task<string> UnprotectAsync(string value);
        Task<bool> ValidateApiKey(string userApiKey);

    }
    public class MessagingSecurityService(IConfiguration configuration, IEncryptionGatewayService encryptionGatewayService) : ServiceBase(configuration), IMessagingSecurityService
    {
        public async Task<bool> ValidateApiKey(string userApiKey)
        {
            if (string.IsNullOrWhiteSpace(userApiKey))
            {
                return false;
            }
            var apiKey = AppSettings.ApiKey;
            return apiKey == userApiKey;
        }

        public async Task<string> ProtectAsync(string value)
        {
            if (value.IsValid() || value.IsGuid())
            {
                return value;
            }
            return await encryptionGatewayService.ProtectAsync(value);
        }

        public async Task<string> UnprotectAsync(string value)
        {
            if (!value.IsValid() && !value.IsGuid()) 
            {
                return value;
            }
            return await encryptionGatewayService.UnprotectAsync(value);
        }
    }


}
