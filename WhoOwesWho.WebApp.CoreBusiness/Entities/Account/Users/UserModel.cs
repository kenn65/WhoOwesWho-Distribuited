using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users
{
    public class UserModel : ResponseModelBase
    {
        public Guid Id { get; set; } = Guid.Empty;

        [Required(ErrorMessage = "Please enter your full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your e-mail address")]
        public string EmailAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your mobile phone number")]
        public string MobilePhoneNumber { get; set; } = string.Empty;
        public bool Admin { get; set; }

        [Required(ErrorMessage = "Please enter your desired password")]
        public string? Password { get; set; } = string.Empty;
        public bool EmailAddressVerified { get; set; }
    }
}
