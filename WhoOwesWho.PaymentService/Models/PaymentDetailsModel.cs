using Newtonsoft.Json;
using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Models.Base;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentDetailsModel : PaymentModelBase
    {
        [JsonProperty("paymentId")]
        public string? PaymentId { get; set; }

        [JsonProperty("eventId")]
        public string? EventId { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("created")]
        public string? Created { get; set; }

        [JsonProperty("creditorIncluded")]
        public bool CreditorIncluded { get; set; }

        [JsonProperty("creditEventUser")]
        public UserModel? CreditEventUser { get; set; }

        [JsonProperty("debitEventUsers")]
        public IEnumerable<UserModel>? DebitEventUsers { get; set; }
    }
}
