using WhoOwesWho.CurrencyService.Services.Base;

namespace WhoOwesWho.CurrencyService.Services
{
    public interface ICurrencySecurityService
    {
        Task<bool> ValidateApiKey(string userApiKey);
    }
    public class CurrencySecurityService(IConfiguration configuration) : ServiceBase(configuration), ICurrencySecurityService
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
