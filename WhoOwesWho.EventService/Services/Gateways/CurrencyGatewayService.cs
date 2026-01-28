using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.Models.Models;

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
                $"{AppSettings.CurrencyMicroServiceBaseAddress}/single/get",
                AppSettings.CurrencyMicroServiceApiKey!,
                false,
                new Dictionary<string, dynamic>
                {
                    { "iso", currencyIso }
                },
                token
            )).Symbol!;
        }
    }
}
