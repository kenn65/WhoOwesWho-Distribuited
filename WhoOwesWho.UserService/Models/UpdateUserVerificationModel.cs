using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.UserService.Models
{
    public class UpdateUserVerificationModel : ModelBase
    {
        public bool NoAdmin { get; set; }
    }
}
