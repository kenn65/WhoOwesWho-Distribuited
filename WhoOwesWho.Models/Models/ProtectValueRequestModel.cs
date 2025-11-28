using WhoOwesWho.Models.Models.Base.ServiceBus.RequestModels.Base;

namespace WhoOwesWho.Models.Models
{
    public class ProtectValueRequestModel : RequestModelBase
    {
        public string Text { get; set; } = string.Empty;
    }
}
