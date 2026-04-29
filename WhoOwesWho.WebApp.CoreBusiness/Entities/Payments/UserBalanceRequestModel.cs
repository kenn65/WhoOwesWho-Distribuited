using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class UserBalanceRequestModel : RequestModelBase
    {
        public string UserId { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
