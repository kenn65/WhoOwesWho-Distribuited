using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.Shared.Models
{
    public class EncryptedCookiesResponseModel : ModelBase
    {
        public string TokenName => ".WhoOwesWho.Token";
        public string? TokenValue { get; set; }

        //public string UserIdName => ".WhoOwesWho.UserId";
        //public string? UserIdValue { get; set; }

        //public string UserEmailAddressName => ".WhoOwesWho.Email";
        //public string? UserEmailAddressValue { get; set; }

        //public string AdminName => ".WhoOwesWho.UserAdmin";
        //public string? AdminValue { get; set; }
    }
}
