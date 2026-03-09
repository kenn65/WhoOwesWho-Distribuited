using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.Gateways;

namespace WhoOwesWho.UserService.Services
{
    public interface IResetPasswordService
    {
        Task<UserModel?> ResetPasswordAsync(ResetPasswordRequestModel request);
        Task<ResetPasswordResponseModel> VerifyResetPassword(string emailAddress, string forgotPasswordToken);
    }

    public class ResetPasswordService(
        IConfiguration configuration,
        IUserQueryRepository userQueryRepository,
        IUserMutationRepository userMutationRepository,
        IUserSecurityService userSecurityService
        ) : ServiceBase(configuration), IResetPasswordService
    {
        public async Task<UserModel?> ResetPasswordAsync(ResetPasswordRequestModel request)
        {
            var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(request.EmailAddress);
            if (user is null)
            {
                return new UserModel
                {
                    Success = false,
                    Message = $"Could not find the account with e-mail address: {request.EmailAddress}"
                };
            }
                        
            user.Password = request.NewPassword;

            var response = await userMutationRepository.UpdateUserAsync(user);
            if (response is null)
                return await Task.FromResult(new UserModel
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please, try again."
                });
            response.Success = true;
            response.Message = "Your password was succesfully reset.";
            return response;
        }

        public async Task<ResetPasswordResponseModel> VerifyResetPassword(string emailAddress, string forgotPasswordToken)
        {
            try
            {
                emailAddress = await userSecurityService.UnprotectAsync(emailAddress);
                var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(emailAddress, true);
                var response = await userQueryRepository.GetForgotPasswordTokenAsync(user!.Id);

                if (response.ForgotPasswordToken == forgotPasswordToken && DateTime.Now <= new DateTime(response.ExpirationTime))
                {
                    return await Task.FromResult(new ResetPasswordResponseModel
                    {
                        Success = true,
                        Message = ""
                    });
                }

                return await Task.FromResult(new ResetPasswordResponseModel
                {
                    Message = "Your reset password link is invalid or expired."
                });
            }
            catch (Exception)
            {
                return await Task.FromResult(new ResetPasswordResponseModel
                {
                    Message = "An error occurred while verifying reset password link."
                });
            }
        }
    }
}
