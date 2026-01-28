using WhoOwesWho.Models.Models.Base.ServiceBus;

namespace WhoOwesWho.Models.Models
{
    public class MessagingRequestModel : RequestModelBase
    {
        public string? ForgotPasswordToken { get; set; }
        public UserModel? User { get; set; }
        public string? Host { get; set; }
        public string? Type { get; set; }
        public string? Code { get; set; }
    }
}
