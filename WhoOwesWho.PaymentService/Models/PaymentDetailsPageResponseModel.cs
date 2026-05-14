using Newtonsoft.Json;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentDetailsPageResponseModel : ModelBase
    {
        [JsonProperty("paymentDetails")]
        public PaymentDetailsModel? PaymentDetails { get; set; }

        [JsonProperty("event")]
        public EventModel? Event { get; set; }

        [JsonProperty("currencies")]
        public IEnumerable<CurrencyResponseModel>? Currencies { get; set; }
        
    }
}
