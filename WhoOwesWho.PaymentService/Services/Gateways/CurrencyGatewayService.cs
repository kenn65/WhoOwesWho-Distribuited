using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Services.Base;

namespace WhoOwesWho.PaymentService.Services.Gateways
{
    public interface ICurrencyGatewayService
    {
        Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync(string token);
        Task<string> GetCurrencySymbolAsync(string currencyIso, string token);
        Task<ExchangeRateResponseModel> GetExchangeRateAsync(string paymentCurrencyIso, string eventCurrencyIso, string token);
    }

    public class CurrencyGatewayService(IConfiguration configuration) : GatewayServiceBase(configuration), ICurrencyGatewayService
    {
        public async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync(string token)
        {
            return await Get<IEnumerable<CurrencyResponseModel>>(
                $"{AppSettings.CurrencyMicroServiceBaseAddress}", 
                AppSettings.CurrencyMicroServiceApiKey!, 
                false, 
                parameters: null, 
                token);
        }

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

        public async Task<ExchangeRateResponseModel> GetExchangeRateAsync(string paymentCurrencyIso, string eventCurrencyIso, string token)
        {
            return (await Get<ExchangeRateResponseModel>(
                $"{AppSettings.CurrencyMicroServiceBaseAddress}/{paymentCurrencyIso}/{eventCurrencyIso}",
                AppSettings.CurrencyMicroServiceApiKey!,
                false,
                parameters: null,
                token
            ));
        }
    }
}
