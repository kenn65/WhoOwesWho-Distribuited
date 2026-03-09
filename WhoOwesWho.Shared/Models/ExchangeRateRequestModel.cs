namespace WhoOwesWho.Shared.Models
{
    public class ExchangeRateRequestModel 
    {
        public string? PaymentCurrencyIso { get; set; }
        public string? EventCurrencyIso { get; set; }
    }
}
