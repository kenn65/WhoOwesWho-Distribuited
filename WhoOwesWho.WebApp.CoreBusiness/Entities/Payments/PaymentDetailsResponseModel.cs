using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.Infrastructure.Currencies;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class PaymentDetailsResponseModel : ResponseModelBase
    {
        public PaymentDetailsModel? PaymentDetails { get; set; }
        public EventModel? Event { get; set; }
        public IEnumerable<CurrencyResponseModel>? Currencies { get; set; }

    }
}
