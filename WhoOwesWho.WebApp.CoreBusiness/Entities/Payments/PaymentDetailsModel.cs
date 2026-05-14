using Mapster;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class PaymentDetailsModel : PaymentResponseModelBase
    {
        public string PaymentId { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Created { get; set; } = string.Empty;
        public bool CreditorIncluded { get; set; }
        public UserMessageResponseModel? CreditEventUser { get; set; }
        public IEnumerable<UserMessageResponseModel>? DebitEventUsers { get; set; }
        public IEnumerable<string>? DebitEventUserIds { get; set; }
    }
}
