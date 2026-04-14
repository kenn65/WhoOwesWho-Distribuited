using StackExchange.Redis;
using System.Text.Json;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.CurrencyService.Repositories
{
    public interface ICurrencyCacheRepository
    {
        Task<IEnumerable<CurrencyResponseModel>?> GetAllCurrenciesAsync();
        
        Task SaveCurrenciesAsync(IEnumerable<CurrencyResponseModel> currencies);
    }

    public class CurrencyCacheRepository(IDatabase db) : ICurrencyCacheRepository
    {
        public async Task<IEnumerable<CurrencyResponseModel>?> GetAllCurrenciesAsync()
        {
            var value = await db.StringGetAsync("currencies");
            return (value.HasValue
                ? JsonSerializer.Deserialize<IEnumerable<CurrencyResponseModel>>(value!.ToString())
                : null);
        }
              
        public async Task SaveCurrenciesAsync(IEnumerable<CurrencyResponseModel> currencies)
        {
            var json = JsonSerializer.Serialize(currencies);
            await db.StringSetAsync("currencies", json, TimeSpan.FromHours(12));
        }
    }
}
