using Newtonsoft.Json;
using WhoOwesWho.PaymentService.Models.Base;

namespace WhoOwesWho.PaymentService.Models
{
    public class CreatePaymentRequestModel : PaymentModelBase
    {
        [JsonProperty("paymentId")]
        public Guid PaymentId { get; set; }

        [JsonProperty("eventId")]
        public Guid EventId { get; set; }
        
        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }
        
        [JsonProperty("creditorId")]
        public Guid CreditorId { get; set; }

        [JsonProperty("debitorId")]
        public Guid DebitorId{ get; set; }

        [JsonProperty("userIds")]
        public IEnumerable<string>? UserIds { get; set; }

        [JsonProperty("creditorIncluded")]
        public bool CreditorIncluded { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }
    }
}
