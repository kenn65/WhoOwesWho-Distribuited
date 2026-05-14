using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.Shared.Models
{
    public class CurrencyResponseModel : ModelBase
    {
        public string? Iso { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
    }
}
