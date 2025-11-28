using WhoOwesWho.Models.Models.Base;

namespace WhoOwesWho.UserService.Models
{
    public class UpdateUserVerificationModel : ModelBase
    {
        public bool NoAdmin { get; set; }
    }
}
