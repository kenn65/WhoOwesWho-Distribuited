using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password
{
    public class ResetPasswordRequestModel : RequestModelBase
    {
        public string EmailAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a new password")]
        public string NewPassword { get; set; } = string.Empty; 
        
        [Required(ErrorMessage = "Please repeat the new password")]
        public string NewPasswordRepeat { get; set; } = string.Empty;
    }
}
