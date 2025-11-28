using Newtonsoft.Json;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EventService.Models
{
    public class EventAssignmentModel
    {
        [JsonProperty("eventId")]
        public Guid EventId { get; set; }
        
        [JsonProperty("user")]
        public UserModel? User { get; set; }
    }
}
