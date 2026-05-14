using System.ComponentModel.DataAnnotations;

namespace WhoOwesWho.PaymentService.EfCore.DataModels
{
    public class PaymentUsers
    {
        [Required]
        public Guid PaymentId { get; set; }

        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        public bool IsCreditor { get; set; }

        [Required]
        public long Created { get; set; }
    }
}
