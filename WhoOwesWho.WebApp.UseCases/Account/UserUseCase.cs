using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.UseCases.Account
{
    public interface IUserUseCase
    {
        Task<UserModel> ExecuteAsync(SignUpRequestModel request);
        Task<UserModel> ExecuteAsync(VerificationRequestModel request);
        Task<UserModel> ExecuteAsync(string id, string jwtToken, bool includePassword = true);
        Task<ForgotPasswordResponseModel> ExecuteAsync(ForgotPasswordRequestModel request);
        Task<ResetPasswordResponseModel> ExecuteAsync(string emailAddress, string forgotPasswordToken);
        Task<ResetPasswordResponseModel> ExecuteAsync(ResetPasswordRequestModel request);
        Task<UserModel> ExecuteAsync(string userId, string jwtToken, UserUpdateRequestModel request);
        Task<ChangePasswordResponseModel> ExecuteAsync(string jwtToken, ChangePasswordRequestModel request);
    }

    public class UserUseCase(IUserPlugin userPlugin, IProtectionUseCase protectionUseCase) : IUserUseCase
    {
        public async Task<UserModel> ExecuteAsync(SignUpRequestModel request)
        {
            request.Entity!.Password = await protectionUseCase.ExecuteProtectAsync(request.Entity.Password!);
            request.Entity.EmailAddress = await protectionUseCase.ExecuteProtectAsync(request.Entity.EmailAddress!);
            return await userPlugin.SignUp(request);
        }

        public async Task<UserModel> ExecuteAsync(VerificationRequestModel request)
        {
            return await userPlugin.VerifyAccountAsync(request);
        }

        public async Task<UserModel> ExecuteAsync(string id, string jwtToken, bool includePassword = true)
        {
            return await userPlugin.GetUserByIdAsync(id, jwtToken, includePassword);
        }

        public async Task<ForgotPasswordResponseModel> ExecuteAsync(ForgotPasswordRequestModel request)
        {
            request.EmailAddress = await protectionUseCase.ExecuteProtectAsync(request.EmailAddress);
            return await userPlugin.ForgotPasswordAsync(request);
        }

        public async Task<ResetPasswordResponseModel> ExecuteAsync(string emailAddress, string forgotPasswordToken)
        {
            return await userPlugin.VerifyResetPasswordAsync(emailAddress, forgotPasswordToken);
        }

        public async Task<ResetPasswordResponseModel> ExecuteAsync(ResetPasswordRequestModel request)
        {
            request.NewPassword = await protectionUseCase.ExecuteProtectAsync(request.NewPassword);
            request.NewPasswordRepeat = await protectionUseCase.ExecuteProtectAsync(request.NewPasswordRepeat);
            return await userPlugin.ResetPasswordAsync(request);
        }

        public async Task<UserModel> ExecuteAsync(string userId, string jwtToken, UserUpdateRequestModel request)
        {
            request.EventId = await protectionUseCase.ExecuteProtectAsync(request.EventId);
            return await userPlugin.UpdateUserAsync(userId, jwtToken, request);
        }

        public async Task<ChangePasswordResponseModel> ExecuteAsync(string jwtToken, ChangePasswordRequestModel request)
        {
            request.Password = await protectionUseCase.ExecuteProtectAsync(request.Password);
            request.NewPassword1 = await protectionUseCase.ExecuteProtectAsync(request.NewPassword1);
            request.NewPassword2 = await protectionUseCase.ExecuteProtectAsync(request.NewPassword2);
            return await userPlugin.ChangePasswordAsync(jwtToken, request);
        }
    }
}
