using WhoOwesWho.Models.Models.Base.ServiceBus;

namespace WhoOwesWho.Models.Models
{
    public class UserRequestModel : RequestModelBase
    {
        public string? IdOrEmailAddress { get; set; }
        public bool IncludePassword { get; set; }
    }
}
