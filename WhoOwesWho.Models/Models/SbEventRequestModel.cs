using WhoOwesWho.Models.Models.Base.ServiceBus.RequestModels.Base;

namespace WhoOwesWho.Models.Models
{
    public class SbEventRequestModel : RequestModelBase
    {
        public string? UserOrEventId { get; set; }
        public bool Active { get; set; }
    }
}
