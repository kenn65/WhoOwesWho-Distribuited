using WhoOwesWho.Models.Models;

namespace WhoOwesWho.CurrencyService.Models
{
    public class CurrencyCachingModel
    {
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
        public IEnumerable<WhoOwesWho.Models.Models.CurrencyResponseModel> Currencies { get; set; } = new List<WhoOwesWho.Models.Models.CurrencyResponseModel>();
        public IDictionary<string, decimal>? ExchangeRates { get; set; } = new Dictionary<string, decimal>();
    }
}
