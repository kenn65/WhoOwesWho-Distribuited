using Newtonsoft.Json;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.Models.Models.Base;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentPageResponseModel : ModelBase
    {
        [JsonProperty("event")]
        public EventResponseModel? Event { get; set; }

        [JsonProperty("payments")]
        public IEnumerable<UserPaymentModel>? Payments { get; set; }

        [JsonProperty("balances")]
        public IEnumerable<UserBalanceResponseModel>? Balances { get; set; }

        [JsonProperty("whoOwesWho")]
        public IEnumerable<WhoOwesWhoModel>? WhoOwesWho { get; set; }
    }
}
