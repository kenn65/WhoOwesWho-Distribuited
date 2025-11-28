using Newtonsoft.Json;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentsRequestModel
    {
        [JsonProperty("userId")] 
        public string? UserId { get; set; }

        [JsonProperty("eventId")]
        public string? EventId { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }
    }
}
