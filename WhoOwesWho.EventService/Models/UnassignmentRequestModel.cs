using Newtonsoft.Json;

namespace WhoOwesWho.EventService.Models
{
    public class UnassignmentRequestModel
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }
    }
}
