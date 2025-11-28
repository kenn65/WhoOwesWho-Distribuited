using Newtonsoft.Json;

namespace WhoOwesWho.CurrencyService.Models
{
    public class ExchangeRateResultModel
    {
        [JsonProperty("data")]
        public Dictionary<string, decimal>? Data { get; set; }
    }
}
