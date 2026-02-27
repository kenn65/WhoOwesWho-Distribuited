using Newtonsoft.Json;
using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Models.Base;

namespace WhoOwesWho.PaymentService.Models
{
    public class UserPaymentResponseModel : PaymentModelBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("eventId")]
        public Guid EventId { get; set; }

        [JsonProperty("creditEventUser")]
        public UserModel? CreditEventUser { get; set; }

        [JsonProperty("debitEventUser")]
        public UserModel? DebitEventUser { get; set; }
        
        [JsonProperty("created")]
        public string? Created { get; set; }

        [JsonProperty("protectedPaymentId")]
        public string? ProtectedPaymentId { get; set; }

        [JsonProperty("protectedCreditUserId")]
        public string? ProtectedCreditUserId { get; set; }
    }
}
