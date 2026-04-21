using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password;

namespace WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces
{
    public interface IUserPlugin
    {
        Task<UserModel> SignUp(SignUpRequestModel request);
        Task<UserModel> VerifyAccountAsync(VerificationRequestModel request);
        Task<UserModel> GetUserByIdAsync(string id, string jwtToken, bool includePassword = true);
        Task<ForgotPasswordResponseModel> ForgotPasswordAsync(ForgotPasswordRequestModel request);
        Task<ResetPasswordResponseModel> VerifyResetPasswordAsync(string emailAddress, string forgotPasswordToken);
        Task<ResetPasswordResponseModel> ResetPasswordAsync(ResetPasswordRequestModel request);

    }
}
