using Newtonsoft.Json;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentDetailsPageRequestModel
    {
        [JsonProperty("paymentId")]
        public string? PaymentId { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }

        
    }
}
