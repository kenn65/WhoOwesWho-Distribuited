using Newtonsoft.Json;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentDetailsPageResponseModel 
    {
        [JsonProperty("paymentDetails")]
        public PaymentDetailsModel? PaymentDetails { get; set; }

        [JsonProperty("event")]
        public EventModel? Event { get; set; }

        [JsonProperty("currencies")]
        public IEnumerable<CurrencyResponseModel>? Currencies { get; set; }
        
    }
}
