using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventRequestModel : RequestModelBase
    {
        public Guid Id { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string Currency { get; set; } = string.Empty;

        public string CurrencySymbol { get; set; } = string.Empty;

        [Required]
        public string? StartDate {get;set;} = string.Empty;

        [Required]
        public DateTime StartDateDate { get; set; }
        
        public long StartDateTicks => StartDateDate.Ticks;
        
        public bool Settled { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public bool AutoAssign { get; set; }
    }
}
