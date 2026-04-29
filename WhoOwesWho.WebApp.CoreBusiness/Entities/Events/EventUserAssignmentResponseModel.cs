using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventUserAssignmentResponseModel
    {
        public Guid EventId { get; set; }
        public UserMessageResponseModel? User { get; set; }
    }
}
