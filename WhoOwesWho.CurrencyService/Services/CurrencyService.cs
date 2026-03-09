using Flurl.Http;
using System.Globalization;
using WhoOwesWho.CurrencyService.Models;
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

    public class CurrencyService(IConfiguration configuration) : ServiceBase(configuration), ICurrencyService
    {
        private static CurrencyCachingModel? _cache = new()
        {
            LastUpdated = default,
            Currencies = new List<CurrencyResponseModel>(),
            ExchangeRates = new Dictionary<string, decimal>()
        };

        public async Task<CurrencyResponseModel> GetCurrencyAsync(string iso)
        {
            var currencies = await GetCurrenciesAsync();
            return await Task.FromResult(currencies!.First(c => c.Iso == iso));
        }

        public async Task<IEnumerable<CurrencyResponseModel>?> GetCurrenciesAsync()
        {
            if (_cache is null || !_cache!.Currencies.Any() || (DateTime.Now - _cache.LastUpdated).TotalDays >= 1)
            {
                await InitializeCache();
            }
            return await Task.FromResult(_cache!.Currencies);
        }

        public async Task<ExchangeRateResponseModel> GetExchangeRateAsync(string paymentCurrencyIso, string eventCurrencyIso)
        {
            if (_cache is null || !_cache!.ExchangeRates!.Any() || (DateTime.Now - _cache.LastUpdated).TotalDays >= 1)
            {
                await InitializeCache();
            }

            var response = new ExchangeRateResponseModel
            {
                ExchangeRate = 1
            };

            if (paymentCurrencyIso == eventCurrencyIso)
            {
                return await Task.FromResult(response);
            }

            response.ExchangeRate = _cache!.ExchangeRates![eventCurrencyIso] / _cache.ExchangeRates[paymentCurrencyIso];
            return await Task.FromResult(response);
        }

        private async Task<IEnumerable<CurrencyResponseModel>> CacheCurrencies()
        {
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
            var response = await client.Request().GetJsonAsync<CurrencyModel?>();
            var isoCurrencySymbols = response!.Data?.Values.Select(item => item.Code).ToList();
            return currencies.Where(c => isoCurrencySymbols!.Contains(c.ISOCurrencySymbol)).Select(c => new CurrencyResponseModel
            {
                Iso = c.ISOCurrencySymbol,
                Name = c.CurrencyEnglishName,
                Symbol = c.CurrencySymbol
            }).OrderBy(currency => currency.Iso).ToList();
        }

        private async Task<IDictionary<string, decimal>?> CacheExchangeRates()
        {
            var endpoint = $"{AppSettings.FreeCurrencyHost}/latest?apikey={AppSettings.FreeCurrencyApiKey}";
            using var client = GetClient(endpoint);
            var result = await client.Request().GetJsonAsync<ExchangeRateResultModel>();
            return result?.Data;
        }

        public async Task InitializeCache()
        {
            _cache = null;
            _cache = new CurrencyCachingModel
            {
                LastUpdated = DateTime.Now,
                Currencies = await CacheCurrencies(),
                ExchangeRates = await CacheExchangeRates()
            };
        }

       
    }
}
