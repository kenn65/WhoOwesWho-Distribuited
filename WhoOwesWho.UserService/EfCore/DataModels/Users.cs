using System.ComponentModel.DataAnnotations;

namespace WhoOwesWho.UserService.EfCore.DataModels
{
    public class Users
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(50)]
        public string? EmailAddress { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string? MobilePhoneNumber { get; set; }

        [Required]
        public bool Admin { get; set; }

        [Required]
        [MaxLength(2000)]
        public string? Password { get; set; }

        [Required]
        public bool EmailAddressVerified { get; set; }
    }
}
