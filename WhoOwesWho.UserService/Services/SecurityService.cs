using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Settings;

namespace WhoOwesWho.UserService.Services
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
