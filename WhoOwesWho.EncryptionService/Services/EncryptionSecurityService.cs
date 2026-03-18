using WhoOwesWho.EncryptionService.Services.Base;
using WhoOwesWho.EncryptionService.Settings;

namespace WhoOwesWho.EncryptionService.Services
{
    public interface IEncryptionSecurityService
    {
        Task<bool> ValidateApiKey(string userApiKey);
    }
    public class EncryptionSecurityService(IConfiguration configuration) : ServiceBase(configuration), IEncryptionSecurityService
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
    }
}
