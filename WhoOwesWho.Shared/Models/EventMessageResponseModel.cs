using Newtonsoft.Json;
using WhoOwesWho.Shared.Extensions;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.Shared.Models
{
    public class EventMessageResponseModel : ModelBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("createdBy")]
        public string? CreatedBy { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("location")]
        public string? Location { get; set; }

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("currencySymbol")]
        public string? CurrencySymbol { get; set; }

        [JsonProperty("startDate")]
        public long StartDate { get; set; }

        [JsonProperty("startDateIso")]
        public string StartDateIso => new DateTime(StartDate).ToDisplayDateFormat();

        [JsonProperty("startDateIsoYmd")]
        public string StartDateIsoYmd => new DateTime(StartDate).ToIsoDateTimeFormat();

        [JsonProperty("settled")]
        public bool Settled { get; set; }

        [JsonProperty("userIds")]
        public IEnumerable<Guid>? UserIds { get; set; }
    }
}
