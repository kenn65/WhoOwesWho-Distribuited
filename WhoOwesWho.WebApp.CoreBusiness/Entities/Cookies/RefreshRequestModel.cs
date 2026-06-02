using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies
{
    public class RefreshRequestModel : RequestModelBase
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
