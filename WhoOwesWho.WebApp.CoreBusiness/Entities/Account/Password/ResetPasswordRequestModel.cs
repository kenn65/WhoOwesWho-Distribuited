using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password
{
    public class ResetPasswordRequestModel : RequestModelBase
    {
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required]
        public string NewPasswordRepeat { get; set; } = string.Empty;
    }
}
