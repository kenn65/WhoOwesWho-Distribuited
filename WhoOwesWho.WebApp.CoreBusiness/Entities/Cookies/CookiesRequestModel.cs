using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies
{
    public class CookiesRequestModel : RequestModelBase
    {
        public UserModel? User { get; set; }
    }
}
