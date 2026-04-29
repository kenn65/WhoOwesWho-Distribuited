using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Extensions.WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class WhoOwesWhoModel : ResponseModelBase
    {
        public string CreditorName { get; set; } = string.Empty;
        public string DebitorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string FormattedAmount => Amount.FormatAmount();
    }
}
