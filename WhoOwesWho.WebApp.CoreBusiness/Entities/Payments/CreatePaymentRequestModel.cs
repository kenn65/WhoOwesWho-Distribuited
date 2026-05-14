using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments.Base;
using WhoOwesWho.WebApp.CoreBusiness.Validation;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class CreatePaymentRequestModel : PaymentRequestModelBase
    {
        public Guid PaymentId { get; set; }

        public Guid EventId { get; set; }

        [Required(ErrorMessage = "Please enter the total amount.")]
        public decimal? TotalAmount { get; set; }

        public Guid CreditorId { get; set; }

        public Guid DebitorId { get; set; }

        
        [MinItems(1, ErrorMessage = "Check the users participating in your payment (maybe including yourself)")]
        public IEnumerable<string>? UserIds { get; set; }


        public bool CreditorIncluded { get; set; }

        public string Token { get; set; } = string.Empty;
    }
}
