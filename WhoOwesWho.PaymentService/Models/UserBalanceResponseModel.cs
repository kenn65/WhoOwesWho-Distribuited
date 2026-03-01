using Newtonsoft.Json;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.PaymentService.Models
{
    public class UserBalanceResponseModel : ModelBase
    {
        [JsonProperty("user")]
        public UserModel? User { get; set; }
        
        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("currencySymbol")]
        public string? CurrencySymbol { get; set; }

        [JsonProperty("formattedBalance")] 
        public string FormattedBalance => Balance.FormatAmount();

    }
}
