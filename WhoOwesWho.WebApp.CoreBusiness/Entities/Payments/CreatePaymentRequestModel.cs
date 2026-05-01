using System.ComponentModel.DataAnnotations;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments.Base;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class CreatePaymentRequestModel : PaymentModelBase
    {
        public Guid PaymentId { get; set; }

        public string EventId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the total amount.")]
        public decimal? TotalAmount { get; set; } 
                
        public string CreditorId { get; set; } = string.Empty;

        public string DebitorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please check users")]
        [MinLength(2, ErrorMessage = "Please chcek at least two users")]
        public IEnumerable<string>? UserIds { get; set; }

        public bool CreditorIncluded { get; set; }
                
        public string Token { get; set; } = string.Empty;
    }
}
