using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Extensions;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventResponseModel : ResponseModelBase
    {
        public Guid Id { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
                
        public string Location { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public string CurrencySymbol { get; set; } = string.Empty;

        public long StartDate { get; set; }

        public DateTime StartDateDate { get; set; }
                
        public string StartDateIso => new DateTime(StartDate).ToDisplayDateFormat();
                
        public string StartDateIsoYmd => new DateTime(StartDate).ToIsoDateTimeFormat();

        public bool Settled { get; set; }

        public IEnumerable<UserModel>? Users { get; set; } = null;
    }
}
