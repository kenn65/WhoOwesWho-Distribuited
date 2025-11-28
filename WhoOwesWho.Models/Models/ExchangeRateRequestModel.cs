using WhoOwesWho.Models.Models.Base.ServiceBus.RequestModels.Base;

namespace WhoOwesWho.Models.Models
{
    public class ExchangeRateRequestModel : RequestModelBase
    {
        public string? PaymentCurrencyIso { get; set; }
        public string? EventCurrencyIso { get; set; }
    }
}
