using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Extensions;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventResponseModel : ResponseModelBase
    {
        public Guid Id { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event location is required")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event currency is required")]
        public string Currency { get; set; } = string.Empty;

        public string CurrencySymbol { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event start date is required")]
        public long StartDate { get; set; }

        public DateTime StartDateDate { get; set; }
                
        public string StartDateIso => new DateTime(StartDate).ToDisplayDateFormat();
                
        public string StartDateIsoYmd => new DateTime(StartDate).ToIsoDateTimeFormat();

        public bool Settled { get; set; }

        public IEnumerable<UserModel>? Users { get; set; } = null;
    }
}
