using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.User
{
    public class UserModel : ModelBase
    {
        public string? ProtectedId { get; set; }
        public Guid Id { get; set; } = Guid.Empty;

        [Required]
        public string? FullName { get; set; }

        [Required]
        public string? EmailAddress { get; set; }

        [Required]
        public string? MobilePhoneNumber { get; set; }

        public bool Admin { get; set; }

        [Required]
        public string? Password { get; set; }
        public bool EmailAddressVerified { get; set; }
    }
}
