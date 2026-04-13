using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;

namespace WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces
{
    public interface IAuthorize
    {
        Task<AuthenticationResponseModel> AuthenticateAsync(AuthenticationRequestModel request);
        Task<AuthorizationResponseModel> AuthorizeAsync(AuthorizationRequestModel request);
    }
}
