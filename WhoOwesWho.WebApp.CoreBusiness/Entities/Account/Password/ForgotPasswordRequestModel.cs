using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password
{
    public class ForgotPasswordRequestModel : RequestModelBase
    {
        [Required(ErrorMessage = "Please enter your e-mail address")]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        public string Host { get; set; } = string.Empty;
    }
}
