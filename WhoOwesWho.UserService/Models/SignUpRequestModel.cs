using WhoOwesWho.Models.Models;

namespace WhoOwesWho.UserService.Models
{
    public class SignUpRequestModel
    {
        public UserModel? Entity { get; set; } 
        public string? Host { get; set; }
    }
}
