using Newtonsoft.Json;

namespace WhoOwesWho.EventService.Models
{
    public class SettleEventRequestModel
    {
        [JsonProperty("eventId")]
        public Guid EventId { get; set; }
    }
}
