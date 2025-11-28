using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.EventService.Settings;

namespace WhoOwesWho.EventService.Services
{
    public interface ISecurityService
    {
        Task<bool> ValidateApiKey(string userApiKey);
    }
    public class SecurityService(IConfiguration configuration) : ServiceBase(configuration), ISecurityService
    {
        public async Task<bool> ValidateApiKey(string userApiKey)
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
