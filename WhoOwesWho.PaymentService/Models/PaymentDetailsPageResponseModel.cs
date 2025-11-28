using Newtonsoft.Json;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.PaymentService.Models
{
    public class PaymentDetailsPageResponseModel 
    {
        [JsonProperty("paymentDetails")]
        public PaymentDetailsModel? PaymentDetails { get; set; }

        [JsonProperty("event")]
        public EventResponseModel? Event { get; set; }

        [JsonProperty("currencies")]
        public IEnumerable<CurrencyResponseModel>? Currencies { get; set; }
        
    }
}
