using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class AuthenticationRequestModel : RequestModelBase
    {
        [Required]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Host { get; set; } = string.Empty;

    }
}
