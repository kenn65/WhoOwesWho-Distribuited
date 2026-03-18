using Newtonsoft.Json;

namespace WhoOwesWho.PaymentService.Models
{
    public class UserPaymentsRequestModel
    {
        [JsonProperty("eventId")]
        public string? EventId { get; set; }

        [JsonProperty("userId")]
        public string? UserId { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }
    }
}
