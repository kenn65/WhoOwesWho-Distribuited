using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.UseCases.Account
{
    public interface IAuthorizationUseCase
    {
        Task<AuthenticationResponseModel> ExecuteAsync(AuthenticationRequestModel request);
        Task<AuthorizationResponseModel> ExecuteAsync(AuthorizationRequestModel request);
    }

    public class AuthorizationUseCase(IAuthorize authorizationPlugin, IProtectionUseCase protectionUseCase) : IAuthorizationUseCase
    {
        public async Task<AuthenticationResponseModel> ExecuteAsync(AuthenticationRequestModel request)
        {
            var requestModel = new AuthenticationRequestModel
            {
                EmailAddress = await protectionUseCase.ExecuteProtectAsync(request.EmailAddress),
                Password = await protectionUseCase.ExecuteProtectAsync(request.Password),
                Host = request.Host
            };
            return await authorizationPlugin.AuthenticateAsync(requestModel);
        }

        public async Task<AuthorizationResponseModel> ExecuteAsync(AuthorizationRequestModel request)
        {
            var requestModel = new AuthorizationRequestModel
            {
                EmailAddress = await protectionUseCase.ExecuteProtectAsync(request.EmailAddress!),
            };  
            return await authorizationPlugin.AuthorizeAsync(requestModel);
        }
    }
}