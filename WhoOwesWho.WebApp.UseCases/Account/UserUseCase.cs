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
        Task<UserModel> ExecuteAsync(Guid id, bool includePassword = true);
        Task<ForgotPasswordResponseModel> ExecuteAsync(ForgotPasswordRequestModel request);
        Task<ResetPasswordResponseModel> ExecuteAsync(string emailAddress, string forgotPasswordToken);
        Task<ResetPasswordResponseModel> ExecuteAsync(ResetPasswordRequestModel request);
        Task<UserModel> ExecuteAsync(UserUpdateRequestModel request);
        Task<ChangePasswordResponseModel> ExecuteAsync(ChangePasswordRequestModel request);
        Task<bool> ExecuteAsync(Guid id);
    }

    public class UserUseCase(IUserPlugin userPlugin, IProtectionUseCase protectionUseCase) : IUserUseCase
    {
        public async Task<UserModel> ExecuteAsync(SignUpRequestModel request)
        {
            request.Entity!.Password = await protectionUseCase.ExecuteProtectAsync(request.Entity.Password!);
            return await userPlugin.SignUp(request);
        }

        public async Task<UserModel> ExecuteAsync(VerificationRequestModel request)
        {
            return await userPlugin.VerifyAccountAsync(request);
        }

        public async Task<UserModel> ExecuteAsync(Guid id, bool includePassword = true)
        {
            return await userPlugin.GetUserByIdAsync(id, includePassword);
        }

        public async Task<ForgotPasswordResponseModel> ExecuteAsync(ForgotPasswordRequestModel request)
        {
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

        public async Task<UserModel> ExecuteAsync(UserUpdateRequestModel request)
        {
            return await userPlugin.UpdateUserAsync(request);
        }

        public async Task<ChangePasswordResponseModel> ExecuteAsync(ChangePasswordRequestModel request)
        {
            request.Password = await protectionUseCase.ExecuteProtectAsync(request.Password);
            request.NewPassword1 = await protectionUseCase.ExecuteProtectAsync(request.NewPassword1);
            request.NewPassword2 = await protectionUseCase.ExecuteProtectAsync(request.NewPassword2);
            return await userPlugin.ChangePasswordAsync(request);
        }

        public async Task<bool> ExecuteAsync(Guid id)
        {
            var response = await userPlugin.GetIsAdminAsync(id);
            return response.IsAdmin;
        }
    }
}
