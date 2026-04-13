using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.Shared.Models
{
    public class AuthenticationResponseModel : ModelBase
    {
        public string? Code { get; set; }
    }
}
