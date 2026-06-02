using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.Infrastructure.Currencies;

namespace WhoOwesWho.WebApp.UseCases.Currencies.PluginInterfaces
{
    public interface ICurrencyPlugin
    {
        Task<EnumerableWrapperResponseModel<IEnumerable<CurrencyResponseModel>>> GetCurrenciesAsync();
    }
}
