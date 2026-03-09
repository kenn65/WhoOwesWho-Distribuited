using WhoOwesWho.Shared.Models.Base.ServiceBus;

namespace WhoOwesWho.Shared.Models
{
    public class MessagingRequestModel : RequestModelBase
    {
        public string? ForgotPasswordToken { get; set; }
        public UserMessageRequestModel? User { get; set; }
        public string? Host { get; set; }
        public string? Type { get; set; }
        public string? Code { get; set; }
    }
}