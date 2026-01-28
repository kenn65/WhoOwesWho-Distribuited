using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Models;
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
        IDataQueryService dataSelectionService,
        IDataMutationService dataModificationService,
        IEncryptionGatewayService encryptionGatewayService
        ) : ServiceBase(configuration), IResetPasswordService
    {
        public async Task<UserModel?> ResetPasswordAsync(ResetPasswordRequestModel request)
        {
            var user = await dataSelectionService.GetSingleUserByEmailAddressAsync(request.EmailAddress);
            if (user == null)
            {
                return new UserModel
                {
                    Success = false,
                    Message = $"Could not find the account with e-mail address: {request.EmailAddress}"
                };
            }

            var protectedPassword = await encryptionGatewayService.ProtectAsync(request.NewPassword!, true);
                        
            user.Password = protectedPassword;

            var response = await dataModificationService.UpdateUserAsync(user);
            if (response == null)
                return await Task.FromResult(new UserModel
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please, try again."
                });
            response.Success = true;
            response.Message = "Password was successfully reset.";
            return await Task.FromResult(response);
        }

        public async Task<ResetPasswordResponseModel> VerifyResetPassword(string emailAddress, string forgotPasswordToken)
        {
            try
            {
                emailAddress = await encryptionGatewayService.ProtectAsync(emailAddress, true);
                var user = await dataSelectionService.GetSingleUserByEmailAddressAsync(emailAddress, true);
                var response = await dataSelectionService.GetForgotPasswordTokenAsync(user!.Id);

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
