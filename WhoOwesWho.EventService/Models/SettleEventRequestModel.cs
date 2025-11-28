using Newtonsoft.Json;

namespace WhoOwesWho.EventService.Models
{
    public class SettleEventRequestModel
    {
        [JsonProperty("eventId")]
        public string? EventId { get; set; }
    }
}
