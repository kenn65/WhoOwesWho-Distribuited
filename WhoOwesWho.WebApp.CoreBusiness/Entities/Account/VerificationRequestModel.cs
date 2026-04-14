using System.ComponentModel.DataAnnotations;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Account
{
    public class VerificationRequestModel
    {
        [Required]
        public string? EmailAddress { get; set; }
    }
}
