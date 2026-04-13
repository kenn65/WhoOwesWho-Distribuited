using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies
{
    public class CookiesResponseModel : ModelBase
    {
        public string TokenName => ".WhoOwesWho.Token";
        public string TokenValue { get; set; } = string.Empty;
        public string UserIdName => ".WhoOwesWho.UserId";
        public string UserIdValue { get; set; } = string.Empty;
        public string UserEmailAddressName => ".WhoOwesWho.Email";
        public string UserEmailAddressValue { get; set; } = string.Empty;
        public string AdminName => ".WhoOwesWho.UserAdmin";
        public string AdminValue { get; set; } = string.Empty;
    }
}
