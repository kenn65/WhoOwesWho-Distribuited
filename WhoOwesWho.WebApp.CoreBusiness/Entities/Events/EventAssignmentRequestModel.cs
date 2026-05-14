using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventAssignmentRequestModel : RequestModelBase
    {
        [Required(ErrorMessage = "Please, select an event")]
        public Guid EventId => Guid.Parse(EventIdString);
        public Guid UserId => Guid.Parse(UserIdString);
        public string EventIdString { get; set; } = string.Empty;
        public string UserIdString { get; set; } = string.Empty;
    }
}
