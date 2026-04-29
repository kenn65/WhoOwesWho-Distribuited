using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class UserPaymentResponseModel : ResponseModelBase
    {
        public Guid Id { get; set; }    
        public Guid EventId { get; set; }
        public UserModel? CreditEventUser { get; set; }
        public UserModel? DebitEventUser { get; set; }
        public string  Created { get; set; } = string.Empty;
        public string ProtectedPaymentId { get; set; } = string.Empty;
        public string? ProtectedCreditUserId { get; set; } = string.Empty;
    }
}
