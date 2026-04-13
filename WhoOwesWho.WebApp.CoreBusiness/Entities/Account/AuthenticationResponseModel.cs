using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class AuthenticationResponseModel : ModelBase
    {
        public string Code { get; set; } = string.Empty;
    }
}
