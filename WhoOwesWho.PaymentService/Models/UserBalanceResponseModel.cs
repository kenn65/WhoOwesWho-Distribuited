using Newtonsoft.Json;
using WhoOwesWho.Shared.Extensions;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.PaymentService.Models
{
    public class    UserBalanceResponseModel : ModelBase
    {
        [JsonProperty("user")]
        public UserMessageResponseModel? User { get; set; }
        
        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("currencySymbol")]
        public string? CurrencySymbol { get; set; }

        [JsonProperty("formattedBalance")] 
        public string FormattedBalance => Balance.FormatAmount();

    }
}
