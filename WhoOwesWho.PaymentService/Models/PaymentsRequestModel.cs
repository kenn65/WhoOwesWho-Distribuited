using Newtonsoft.Json;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentsRequestModel
    {
        [JsonProperty("eventId")]
        public Guid EventId { get; set; }
        
        [JsonProperty("active")]
        public bool Active { get; set; }
    }
}
