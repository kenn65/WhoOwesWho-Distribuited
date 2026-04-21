using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password
{
    public class ChangePasswordRequestModel : RequestModelBase
    {
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string NewPassword1 { get; set; } = string.Empty;

        [Required]
        public string NewPassword2 { get; set; } = string.Empty;
    }
}
