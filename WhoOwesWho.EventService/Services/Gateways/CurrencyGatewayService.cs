using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Services.Gateways
{
    public interface ICurrencyGatewayService
    {
        Task<string> GetCurrencySymbolAsync(string currencyIso, string token);
    }
    public class CurrencyGatewayService(IConfiguration configuration) : GatewayServiceBase(configuration), ICurrencyGatewayService
    {
        public async Task<string> GetCurrencySymbolAsync(string currencyIso, string token)
        {
            return (await Get<CurrencyResponseModel>(
                $"{AppSettings.CurrencyMicroServiceBaseAddress}/{currencyIso}",
                AppSettings.CurrencyMicroServiceApiKey!,
                false,
                parameters: null,
                token
            )).Symbol!;
        }
    }
}
