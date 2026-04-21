using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class AuthenticationResponseModel : ResponseModelBase
    {
        public string Code { get; set; } = string.Empty;
    }
}
