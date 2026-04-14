using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.UseCases.Account
{
    public interface IUserUseCase
    {
        Task<UserModel> ExecuteSignUpAsync(SignUpRequestModel request);
        Task<UserModel> VerifyAsync(VerificationRequestModel request);
    }

    public class UserUseCase(IUser userPlugin, IProtectionUseCase protectionUseCase) : IUserUseCase
    {
        public async Task<UserModel> ExecuteSignUpAsync(SignUpRequestModel request)
        {
            var requestModel = new SignUpRequestModel
            {
                Entity = request.Entity,
                Host = request.Host
            };
            requestModel.Entity!.Password = await protectionUseCase.ExecuteProtectAsync(requestModel.Entity.Password!);
            requestModel.Entity.EmailAddress = await protectionUseCase.ExecuteProtectAsync(requestModel.Entity.EmailAddress!);
            return await userPlugin.SignUp(requestModel);
        }

        public async Task<UserModel> VerifyAsync(VerificationRequestModel request)
        {
            return await userPlugin.VerifyAsync(request);
        }
    }
}
