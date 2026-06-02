using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;

namespace WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces
{
    public interface IAuthorizationPlugin
    {
        Task<AuthenticationResponseModel> AuthenticateAsync(AuthenticationRequestModel request);
        Task<AuthorizationResponseModel> AuthorizeAsync(AuthorizationRequestModel request);
        Task<CookiesResponseModel> RefreshAsync();
        Task<CookiesDeletionResponseModel> DeleteCookiesAsync();

    }
}
