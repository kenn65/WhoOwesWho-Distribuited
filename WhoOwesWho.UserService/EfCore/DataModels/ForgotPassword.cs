using System.ComponentModel.DataAnnotations;

namespace WhoOwesWho.UserService.EfCore.DataModels
{
    public class ForgotPassword
    {
        [Required]
        public Guid UserId { get; set; }


        [Required]
        public long ExpirationTime { get; set; }

        [Required]
        [MaxLength(500)]
        public string? ForgotPasswordToken { get; set; } 

        
    }
}
