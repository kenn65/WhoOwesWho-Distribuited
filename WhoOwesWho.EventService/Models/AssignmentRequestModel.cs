using Newtonsoft.Json;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EventService.Models
{
    public class AssignmentRequestModel
    {
        [JsonProperty("eventId")]
        public string? EventId { get; set; }

        [JsonProperty("userId")]
        public string? UserId { get; set; }
        
        [JsonProperty("user")]
        public UserModel? User { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }

        
    }
}
