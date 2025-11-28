using WhoOwesWho.Models.Models.Base;

namespace WhoOwesWho.AuthorizationService.Models
{
    public class AuthenticationResponseModel : ModelBase
    {
        public string? Code { get; set; }
    }
}
