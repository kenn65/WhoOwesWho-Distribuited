using Newtonsoft.Json;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentDetailsPageRequestModel
    {
        [JsonProperty("paymentId")]
        public Guid PaymentId { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }
    }
}
