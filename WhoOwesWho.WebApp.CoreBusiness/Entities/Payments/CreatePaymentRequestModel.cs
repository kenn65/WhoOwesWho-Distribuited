using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class CreatePaymentRequestModel : PaymentModelBase
    {
        public Guid PaymentId { get; set; }

        [Required]
        public string EventId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the total amount.")]
        public decimal? TotalAmount { get; set; } 

        [Required]
        public string CreditorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please all users that gained for this payment")]
        public string DebitorId { get; set; } = string.Empty;

        [Required]
        public IEnumerable<string>? UserIds { get; set; }

        [Required]
        public bool CreditorIncluded { get; set; }
                
        public string Token { get; set; } = string.Empty;
    }
}
