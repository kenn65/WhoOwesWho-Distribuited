using Newtonsoft.Json;
using WhoOwesWho.PaymentService.Models.Base;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentDetailsModel : PaymentModelBase
    {
        [JsonProperty("paymentId")]
        public Guid PaymentId { get; set; }

        [JsonProperty("eventId")]
        public Guid EventId { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("created")]
        public string? Created { get; set; }

        [JsonProperty("creditorIncluded")]
        public bool CreditorIncluded { get; set; }

        [JsonProperty("creditEventUser")]
        public UserMessageResponseModel? CreditEventUser { get; set; }

        [JsonProperty("debitEventUsers")]
        public IEnumerable<UserMessageResponseModel>? DebitEventUsers { get; set; }
    }
}
