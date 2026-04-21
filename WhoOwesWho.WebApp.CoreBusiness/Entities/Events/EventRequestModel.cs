using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventRequestModel : RequestModelBase
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public string? StartDate {get;set;} = string.Empty; 
        public DateTime StartDateDate { get; set; }
        public long StartDateTicks => StartDateDate.Ticks;
        public bool Settled { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public bool AutoAssign { get; set; }
    }
}
