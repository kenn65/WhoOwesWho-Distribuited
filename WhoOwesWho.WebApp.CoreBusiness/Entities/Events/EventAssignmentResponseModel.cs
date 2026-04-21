using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventAssignmentResponseModel : ResponseModelBase
    {
        public Guid EventId { get; set; }
        public UserMessageResponseModel? User { get; set; }
    }
}

