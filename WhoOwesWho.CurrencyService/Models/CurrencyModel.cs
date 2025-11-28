using Newtonsoft.Json;

namespace WhoOwesWho.CurrencyService.Models
{
    public class CurrencyModel
    {
        [JsonProperty("data")]
        public Dictionary<string, CurrencyInfo>? Data { get; set; }
    }

    public class CurrencyInfo
    {
        [JsonProperty("symbol")]
        public string? Symbol { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("symbol_native")]
        public string? SymbolNative { get; set; }

        [JsonProperty("decimal_digits")]
        public int DecimalDigits { get; set; }

        [JsonProperty("rounding")]
        public int Rounding { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("name_plural")]
        public string? NamePlural { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }
    }
}
