using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.UseCases.Currencies.PluginInterfaces;

namespace WhoOwesWho.WebApp.UseCases.Currencies
{
    public interface ICurrenciesUseCase
    {
        Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync(string jwtToken);
    }

    public class CurrenciesUseCase(ICurrencyPlugin currencyPlugin) : ICurrenciesUseCase
    {
        public async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync(string jwtToken)
        {
            return await currencyPlugin.GetCurrenciesAsync(jwtToken);
        }
    }
}
