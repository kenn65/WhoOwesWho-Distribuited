using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Extensions.WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments.Base
{
    public class PaymentResponseModelBase : ResponseModelBase
    {
        public decimal Amount { get; set; }
        public string FormattedAmount => Amount.FormatAmount();
        public string? Currency { get; set; } 
        public decimal OriginalAmount { get; set; }
        public string FormattedOriginalAmount => OriginalAmount.FormatAmount();
        public string? OriginalCurrency { get; set; } 
        public string? Description { get; set; } 
    }
}
