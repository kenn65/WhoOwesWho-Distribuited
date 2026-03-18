using Flurl.Http;
using System.Globalization;
using WhoOwesWho.CurrencyService.Models;
using WhoOwesWho.CurrencyService.Repositories;
using WhoOwesWho.CurrencyService.Services.Base;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.CurrencyService.Services
{
    public interface ICurrencyService
    {
        Task<CurrencyResponseModel> GetCurrencyAsync(string iso);
        Task<IEnumerable<CurrencyResponseModel>?> GetCurrenciesAsync();
        Task<ExchangeRateResponseModel> GetExchangeRateAsync(string paymentCurrencyIso, string eventCurrencyIso);
    }

    public class CurrencyService(IConfiguration configuration, ICurrencyCacheRepository currencyCacheRepository) : ServiceBase(configuration), ICurrencyService
    {
        public async Task<CurrencyResponseModel> GetCurrencyAsync(string iso)
        {
            var currencies = await GetCurrenciesAsync();
            return currencies!.First(c => c.Iso == iso);
        }

        public async Task<IEnumerable<CurrencyResponseModel>?> GetCurrenciesAsync()
        {
            var cachedCurrencies = await currencyCacheRepository.GetAllCurrenciesAsync();
            if (cachedCurrencies != null && cachedCurrencies.Any())
            {
                return cachedCurrencies;
            }

            var currencies = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(c => c.Name).Distinct()
                .Select(c => new RegionInfo(c))
                .GroupBy(c => c.ISOCurrencySymbol)
                .Select(c => c.First())
                .Select(c => new
                {
                    c.ISOCurrencySymbol,
                    c.CurrencyEnglishName,
                    c.CurrencySymbol,
                });

            var endpoint = $"{AppSettings.FreeCurrencyHost}/currencies?apikey={AppSettings.FreeCurrencyApiKey}";
            using var client = GetClient(endpoint);
            var currencyModel = await client.Request().GetJsonAsync<CurrencyModel?>();
            var isoCurrencySymbols = currencyModel!.Data?.Values.Select(item => item.Code).ToList();
            var response = currencies.Where(c => isoCurrencySymbols!.Contains(c.ISOCurrencySymbol)).Select(c => new CurrencyResponseModel
            {
                Iso = c.ISOCurrencySymbol,
                Name = c.CurrencyEnglishName,
                Symbol = c.CurrencySymbol
            }).OrderBy(currency => currency.Iso).ToList();
            //Cache the currencies in Redis for 12 hours
            await currencyCacheRepository.SaveCurrenciesAsync(response);
            return response;
        }

        public async Task<ExchangeRateResponseModel> GetExchangeRateAsync(string paymentCurrencyIso, string eventCurrencyIso)
        {
            var response = new ExchangeRateResponseModel
            {
                ExchangeRate = 1
            };
            if (paymentCurrencyIso == eventCurrencyIso)
            {
                return response;
            }
            
            decimal rate = 1;
            var endpoint = $"{AppSettings.FreeCurrencyHost}/latest?apikey={AppSettings.FreeCurrencyApiKey}&base_currency={paymentCurrencyIso}&currencies={eventCurrencyIso}";
            using IFlurlClient client = new FlurlClient(endpoint);
            var exchangeRateResultModel = await client.Request().GetJsonAsync<ExchangeRateResultModel>();
            rate = exchangeRateResultModel.Data!.Values.First();
            response.ExchangeRate = rate;
            return response;
        }
    }
}
