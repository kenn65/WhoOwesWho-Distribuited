using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;

namespace WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces
{
    public interface IAuthorizationPlugin
    {
        Task<AuthenticationResponseModel> AuthenticateAsync(AuthenticationRequestModel request);
        Task<AuthorizationResponseModel> AuthorizeAsync(AuthorizationRequestModel request);
       


    }
}
