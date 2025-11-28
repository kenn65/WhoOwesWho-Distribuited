using Newtonsoft.Json;
using WhoOwesWho.Models.Models.Extensions;

namespace WhoOwesWho.PaymentService.Models
{
    public class WhoOwesWhoModel
    {
        [JsonProperty("creditorName")]
        public string? CreditorName { get; set; }

        [JsonProperty("debitorName")]
        public string? DebitorName { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("formattedAmount")]
        public string FormattedAmount => Amount.FormatAmount();

        
    }
}
