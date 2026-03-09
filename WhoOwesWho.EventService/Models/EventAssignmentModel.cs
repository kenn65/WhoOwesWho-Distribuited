using Newtonsoft.Json;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.EventService.Models
{
    public class EventAssignmentModel : ModelBase
    {
        [JsonProperty("eventId")]
        public Guid EventId { get; set; }
        
        [JsonProperty("user")]
        public UserMessageResponseModel? User { get; set; }
    }
}
