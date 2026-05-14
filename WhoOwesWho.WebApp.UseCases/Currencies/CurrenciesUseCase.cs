using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.UseCases.Currencies.PluginInterfaces;

namespace WhoOwesWho.WebApp.UseCases.Currencies
{
    public interface ICurrenciesUseCase
    {
        Task<EnumerableWrapperResponseModel<IEnumerable<CurrencyResponseModel>>> ExecuteAsync(string jwtToken);
    }

    public class CurrenciesUseCase(ICurrencyPlugin currencyPlugin) : ICurrenciesUseCase
    {
        public async Task<EnumerableWrapperResponseModel<IEnumerable<CurrencyResponseModel>>> ExecuteAsync(string jwtToken)
        {
            return await currencyPlugin.GetCurrenciesAsync(jwtToken);
        }
    }
}
