using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.UseCases.Account
{
    public interface IAuthorizationUseCase
    {
        Task<AuthenticationResponseModel> ExecuteAuthenticateAsync(AuthenticationRequestModel request);
        Task<AuthorizationResponseModel> ExecuteAuthorizeAsync(AuthorizationRequestModel request);
        Task<CookiesResponseModel> ExecuteRefreshTokenAsync();
        Task<CookiesDeletionResponseModel> ExecuteDeleteCookiesAsync();
    }

    public class AuthorizationUseCase(IAuthorizationPlugin authorizationPlugin, IProtectionUseCase protectionUseCase) : IAuthorizationUseCase
    {
        public async Task<AuthenticationResponseModel> ExecuteAuthenticateAsync(AuthenticationRequestModel request)
        {
            var requestModel = new AuthenticationRequestModel
            {
                EmailAddress = request.EmailAddress,
                Password = await protectionUseCase.ExecuteProtectAsync(request.Password),
                Host = request.Host
            };
            return await authorizationPlugin.AuthenticateAsync(requestModel);
        }

        public async Task<AuthorizationResponseModel> ExecuteAuthorizeAsync(AuthorizationRequestModel request)
        {
            return await authorizationPlugin.AuthorizeAsync(request);
        }
        
        public async Task<CookiesResponseModel> ExecuteRefreshTokenAsync()
        {
            return await authorizationPlugin.RefreshAsync();
        }

        public async Task<CookiesDeletionResponseModel> ExecuteDeleteCookiesAsync()
        {
            return await authorizationPlugin.DeleteCookiesAsync();
        }
    }
}