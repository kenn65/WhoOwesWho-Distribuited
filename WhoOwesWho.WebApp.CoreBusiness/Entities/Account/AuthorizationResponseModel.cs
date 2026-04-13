using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class AuthorizationResponseModel : ModelBase
    {
        public static string TokenName => ".WhoOwesWho.Token";
        public string? TokenValue { get; set; }
        public static string UserIdName => ".WhoOwesWho.UserId";
        public string? UserIdValue { get; set; }
        public static string UserEmailAddressName => ".WhoOwesWho.Email";
        public string? UserEmailAddressValue { get; set; }
        public static string AdminName => ".WhoOwesWho.UserAdmin";
        public string? AdminValue { get; set; }
    }
}
