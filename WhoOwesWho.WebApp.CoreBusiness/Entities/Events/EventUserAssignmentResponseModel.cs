using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventUserAssignmentResponseModel : ResponseModelBase
    {
        public Guid EventId { get; set; }
        public UserMessageResponseModel? User { get; set; }
    }
}
