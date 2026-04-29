using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Extensions.WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class UserBalanceResponseModel : ResponseModelBase
    {
        public UserMessageResponseModel? User { get; set; }
            
        public decimal Balance { get; set; }

        public string CurrencySymbol { get; set; } = string.Empty;
                
        public string FormattedBalance => Balance.FormatAmount();
    }
}
