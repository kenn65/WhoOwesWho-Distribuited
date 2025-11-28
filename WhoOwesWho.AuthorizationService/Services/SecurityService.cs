using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Settings;

namespace WhoOwesWho.AuthorizationService.Services
{
    public class SecurityService(IConfiguration configuration) : ServiceBase(configuration)
    {   
        private async Task<bool> ValidateApiKey(string userApiKey)
        {
            if (string.IsNullOrWhiteSpace(userApiKey))
            {
                return false;
            }
            var apiKey = AppSettings.ApiKey;
            return await Task.FromResult(apiKey == userApiKey);
        }
    }
}
