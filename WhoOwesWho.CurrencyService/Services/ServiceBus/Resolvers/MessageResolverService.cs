using System.ComponentModel.DataAnnotations;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.CurrencyService.Services.ServiceBus.Resolvers
{
    public interface IMessageResolverService
    {
        Task<IEnumerable<CurrencyResponseModel>?> GetCurrenciesAsync([Required] string apiKey);
        Task<CurrencyResponseModel> GetCurrencyAsync([Required] string apiKey, [Required] string iso);
        Task<ExchangeRateResponseModel> GetExchangeRateAsync([Required] string apiKey, [Required] string paymentCurrencyIso, [Required] string eventCurrencyIso);
    }
    public class MessageResolverService (ISecurityService securityService, ICurrencyService currencyService) : IMessageResolverService
    {
        public async Task<IEnumerable<CurrencyResponseModel>?> GetCurrenciesAsync([Required] string apiKey)
        {
            if (!await securityService.ValidateApiKey(apiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key provided.");
            }
            return await currencyService.GetCurrenciesAsync();
        }
        public async Task<CurrencyResponseModel> GetCurrencyAsync([Required] string apiKey, [Required] string iso)
        {
            if (!await securityService.ValidateApiKey(apiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key provided.");
            }
            return await currencyService.GetCurrencyAsync(iso);
        }
        public async Task<ExchangeRateResponseModel> GetExchangeRateAsync([Required] string apiKey, [Required] string paymentCurrencyIso, [Required] string eventCurrencyIso)
        {
            if (!await securityService.ValidateApiKey(apiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key provided.");
            }
            return await currencyService.GetExchangeRateAsync(paymentCurrencyIso, eventCurrencyIso);
        }
    }
}
