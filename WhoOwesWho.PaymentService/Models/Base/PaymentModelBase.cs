using Newtonsoft.Json;
using WhoOwesWho.Shared.Extensions;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.PaymentService.Models.Base
{
    public class PaymentModelBase : ModelBase
    {
        [JsonProperty("amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("formattedAmount")]
        public string FormattedAmount => Amount!.Value.FormatAmount();

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("originalAmount")]
        public decimal OriginalAmount { get; set; }

        [JsonProperty("formattedOriginalAmount")]
        public string? FormattedOriginalAmount => OriginalAmount.FormatAmount();

        [JsonProperty("originalCurrency")]
        public string? OriginalCurrency { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }
    }
}
