using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class AuthenticationCodeRequestModel : RequestModelBase
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
