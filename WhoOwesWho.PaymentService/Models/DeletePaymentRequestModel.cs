using Newtonsoft.Json;

namespace WhoOwesWho.PaymentService.Models
{
    public class DeletePaymentRequestModel
    {
        [JsonProperty("paymentId")]
        public string? PaymentId { get; set; }
    }
}
