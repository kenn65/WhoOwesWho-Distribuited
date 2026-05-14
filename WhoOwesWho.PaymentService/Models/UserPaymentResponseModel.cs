using Newtonsoft.Json;
using WhoOwesWho.PaymentService.Models.Base;
using WhoOwesWho.Shared.Models;

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
    }
}
