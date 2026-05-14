using Newtonsoft.Json;
using WhoOwesWho.Shared.Models.Base.ServiceBus;

namespace WhoOwesWho.Shared.Models
{
    public class EventRequestModel : RequestModelBase
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
        public string? StartDate { get; set; }

        public long StartDateTicks => DateTime.Parse(StartDate!).Ticks;

        [JsonProperty("settled")]
        public bool Settled { get; set; }
        
        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public string? Token { get; set; }

        [JsonProperty("autoAssign")]
        public bool AutoAssign { get; set; }


    }
}
