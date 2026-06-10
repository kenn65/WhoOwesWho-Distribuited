using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;

namespace WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces
{
    public interface IUserPlugin
    {
        Task<UserModel> SignUp(SignUpRequestModel request);
        Task<UserModel> VerifyAccountAsync(VerificationRequestModel request);
        Task<UserModel> GetUserByIdAsync(Guid id, bool includePassword = true);
        Task<ForgotPasswordResponseModel> ForgotPasswordAsync(ForgotPasswordRequestModel request);
        Task<ResetPasswordResponseModel> VerifyResetPasswordAsync(string emailAddress, string forgotPasswordToken);
        Task<ResetPasswordResponseModel> ResetPasswordAsync(ResetPasswordRequestModel request);
        Task<UserModel> UpdateUserAsync(UserUpdateRequestModel request);
        Task<ChangePasswordResponseModel> ChangePasswordAsync(ChangePasswordRequestModel request);
        Task<IsAdminResponseModel> GetIsAdminAsync(Guid id);    



    }
}
