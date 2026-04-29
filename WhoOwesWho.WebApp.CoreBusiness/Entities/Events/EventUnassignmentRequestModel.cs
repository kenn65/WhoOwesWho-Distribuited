using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventUnassignmentRequestModel : RequestModelBase
    {
        public string EventId { get; set; } = string.Empty;
        public string? UserId { get; set; } = string.Empty;
    }
}
