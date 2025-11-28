using WhoOwesWho.Models.Models.Base.ServiceBus.RequestModels.Base;

namespace WhoOwesWho.Models.Models
{
    public class CookiesRequestModel : RequestModelBase
    {
        public UserModel? User { get; set; }
    }
}
