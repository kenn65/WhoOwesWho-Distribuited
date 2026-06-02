using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.UseCases.Currencies.PluginInterfaces;

namespace WhoOwesWho.WebApp.UseCases.Currencies
{
    public interface ICurrenciesUseCase
    {
        Task<IEnumerable<CurrencyResponseModel>> ExecuteAsync();
    }

    public class CurrenciesUseCase(ICurrencyPlugin currencyPlugin) : ICurrenciesUseCase
    {
        public async Task<IEnumerable<CurrencyResponseModel>> ExecuteAsync()
        {
            return (await currencyPlugin.GetCurrenciesAsync())?.Data!;
        }
    }
}
