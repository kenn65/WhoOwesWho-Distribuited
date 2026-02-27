using System.ComponentModel.DataAnnotations;

namespace WhoOwesWho.EventService.EfCore.DataModels
{
    public class Events
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Location { get; set; }

        [Required]
        [MaxLength(5)]
        public string? Currency { get; set; }

        [Required]
        [MaxLength(5)]
        public string? CurrencySymbol { get; set; }

        [Required]
        public long StartDate { get; set; }

        [Required]  
        public bool Settled { get; set; }
    }
}
