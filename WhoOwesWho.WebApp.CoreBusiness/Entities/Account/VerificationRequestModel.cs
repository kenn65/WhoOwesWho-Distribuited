using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class VerificationRequestModel : RequestModelBase
    {
        [Required]
        public string? EmailAddress { get; set; }
    }
}
