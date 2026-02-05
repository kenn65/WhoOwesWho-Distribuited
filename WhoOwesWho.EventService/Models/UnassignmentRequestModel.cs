using Newtonsoft.Json;

namespace WhoOwesWho.EventService.Models
{
    public class UnassignmentRequestModel
    {
        public string? EventId { get; set; }
        public string? UserId { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }
    }
}
