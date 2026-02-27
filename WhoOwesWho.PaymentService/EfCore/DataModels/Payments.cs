using System.ComponentModel.DataAnnotations;

namespace WhoOwesWho.PaymentService.EfCore.DataModels
{
    public class Payments
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [Required]
        public Decimal Amount { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(5)]
        public string? Currency { get; set; }

        [Required]
        public decimal OriginalAmount { get; set; }

        [Required]
        [MaxLength(5)]
        public string? OriginalCurrency { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Description { get; set; }

        [Required]
        public long Created { get; set; }

        [Required]
        public bool CreditorIncluded { get; set; }
    }
}
