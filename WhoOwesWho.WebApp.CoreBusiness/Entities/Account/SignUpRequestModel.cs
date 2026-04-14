using System.ComponentModel.DataAnnotations;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class SignUpRequestModel
    {
        [Required]
        public UserModel? Entity { get; set; }

        [Required]
        public string? Host { get; set; }
    }
}
