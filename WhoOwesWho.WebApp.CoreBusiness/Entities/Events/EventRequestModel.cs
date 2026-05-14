using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventRequestModel : RequestModelBase
    {
        public Guid Id { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter an event name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the event location")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select the evemt settlement currency")]
        public string Currency { get; set; } = string.Empty;

        public string CurrencySymbol { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Please select the event start date")]
        public string? StartDate { get; set; } = string.Empty;

        [Required]
        public DateTime StartDateDate { get; set; }

        public long StartDateTicks => StartDateDate.Ticks;

        public bool Settled { get; set; }

        public Guid UserId { get; set; }

        public bool AutoAssign { get; set; }

        public string Token { get; set; } = string.Empty;
    }
}
