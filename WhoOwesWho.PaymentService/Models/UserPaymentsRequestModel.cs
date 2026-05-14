using Newtonsoft.Json;

namespace WhoOwesWho.PaymentService.Models
{
    public class UserPaymentsRequestModel
    {
        [JsonProperty("eventId")]
        public Guid EventId { get; set; }

        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }
    }
}
