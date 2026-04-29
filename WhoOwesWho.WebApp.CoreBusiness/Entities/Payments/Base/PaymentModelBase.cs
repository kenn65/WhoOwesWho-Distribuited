using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Extensions.WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments.Base
{
    public class PaymentModelBase : RequestModelBase
    {
        public decimal Amount { get; set; }
        public string FormattedAmount => Amount.FormatAmount();

        [Required(ErrorMessage = "Please select a currency")]
        public string Currency { get; set; } = string.Empty;
        public decimal OriginalAmount { get; set; }
        public string FormattedOriginalAmount => OriginalAmount.FormatAmount();
        public string OriginalCurrency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a description")]
        public string Description { get; set; } = string.Empty;
    }
}
