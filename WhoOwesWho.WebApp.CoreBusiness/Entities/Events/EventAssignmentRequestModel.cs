using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Events
{
    public class EventAssignmentRequestModel : RequestModelBase
    {
        [Required(ErrorMessage = "Please, select an event")]
        public string EventId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public UserMessageResponseModel? User { get; set; }
    }
}
