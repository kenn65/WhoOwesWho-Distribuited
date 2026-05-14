using Newtonsoft.Json;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Models
{
    public class AssignmentRequestModel
    {
        [JsonProperty("eventId")]
        public Guid EventId { get; set; }

        [JsonProperty("userId")]
        public Guid UserId { get; set; }
        
        [JsonProperty("user")]
        public UserMessageResponseModel? User { get; set; }
    }
}
