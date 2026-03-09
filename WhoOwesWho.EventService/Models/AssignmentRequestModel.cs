using Newtonsoft.Json;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EventService.Models
{
    public class AssignmentRequestModel
    {
        [JsonProperty("eventId")]
        public string? EventId { get; set; }

        [JsonProperty("userId")]
        public string? UserId { get; set; }
        
        [JsonProperty("user")]
        public UserMessageResponseModel? User { get; set; }
    }
}
