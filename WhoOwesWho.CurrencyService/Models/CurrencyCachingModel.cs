using System.Collections.Concurrent;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.CurrencyService.Models
{
    public class CurrencyCachingModel
    {
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
        public IEnumerable<CurrencyResponseModel> Currencies { get; set; } = [];
        public IDictionary<string, decimal>? ExchangeRates { get; set; } = new ConcurrentDictionary<string, decimal>();
    }
}
