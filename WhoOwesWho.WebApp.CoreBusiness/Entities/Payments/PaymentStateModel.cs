namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class PaymentStateModel
    {
        public Guid PaymentId { get; set; }
        public Guid CreditUserId { get; set; } 
        public bool Active { get; set; }
    }
}
