using Newtonsoft.Json;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentsRequestModel
    {
        [JsonProperty("eventId")]
        public string? EventId { get; set; }
        
        [JsonProperty("active")]
        public bool Active { get; set; }
    }
}
