using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;

namespace WhoOwesWho.WebApp.Infrastructure.Currencies
{
    public class CurrencyResponseModel : ResponseModelBase
    {
        public string Iso { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
    }
}
