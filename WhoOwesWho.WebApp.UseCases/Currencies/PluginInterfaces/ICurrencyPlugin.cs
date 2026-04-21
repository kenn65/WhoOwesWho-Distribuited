using WhoOwesWho.WebApp.Infrastructure.Currencies;

namespace WhoOwesWho.WebApp.UseCases.Currencies.PluginInterfaces
{
    public interface ICurrencyPlugin
    {
        Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync(string jwtToken);
    }
}
