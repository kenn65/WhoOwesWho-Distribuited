using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class SettleEventRequestModel : RequestModelBase
    {
        public Guid EventId => Guid.Parse(EventIdString);

        public string EventIdString { get; set; } = string.Empty;
    }
}
