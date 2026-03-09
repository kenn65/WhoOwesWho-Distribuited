using Newtonsoft.Json;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentPageResponseModel : ModelBase
    {
        [JsonProperty("event")]
        public EventModel? Event { get; set; }

        [JsonProperty("payments")]
        public IEnumerable<UserPaymentResponseModel>? Payments { get; set; }

        [JsonProperty("balances")]
        public IEnumerable<UserBalanceResponseModel>? Balances { get; set; }

        [JsonProperty("whoOwesWho")]
        public IEnumerable<WhoOwesWhoModel>? WhoOwesWho { get; set; }
    }
}
