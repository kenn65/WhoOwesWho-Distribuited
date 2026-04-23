using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users
{
    public class UserProfileResponseModel : ResponseModelBase
    {
        [Required(ErrorMessage = "Please enter your full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your e-mail address")]
        public string EmailAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your mobile phone number")]
        public string MobilePhoneNumber { get; set; } = string.Empty;
        public bool Admin { get; set; }
    }
}
