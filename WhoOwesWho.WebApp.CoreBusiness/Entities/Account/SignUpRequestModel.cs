using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class SignUpRequestModel : RequestModelBase
    {
        [Required]
        public UserModel? Entity { get; set; }

        [Required]
        public string? Host { get; set; }
    }
}
