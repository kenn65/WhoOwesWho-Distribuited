using Mapster;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IResetPasswordService
    {
        Task<ResetPasswordResponseModel?> ResetPasswordAsync(ResetPasswordRequestModel request);
        Task<ResetPasswordResponseModel> VerifyResetPassword(string emailAddress, string forgotPasswordToken);
    }

    public class ResetPasswordService(
        IConfiguration configuration,
        IUserValidationService userValidationService,
        IUserLookupService userLookupService,
        IUserQueryRepository userQueryRepository,
        IUserCommandService userCommandService,
        IUserSecurityService userSecurityService
        ) : ServiceBase(configuration), IResetPasswordService
    {
        public async Task<ResetPasswordResponseModel?> ResetPasswordAsync(ResetPasswordRequestModel request)
        {
            var response = new ResetPasswordResponseModel();
            var emailAddress = await userSecurityService.UnprotectAsync(request.EmailAddress!);
            var newPassword = await userSecurityService.UnprotectAsync(request.NewPassword!);
            var newPasswordRepeat = await userSecurityService.UnprotectAsync(request.NewPasswordRepeat!);

            if (newPassword != newPasswordRepeat)
            {
                response.Message = "The passwords does not match!";
                return response;
            }

            var user = await userLookupService.GetSingleUserByEmailAddressAsync(emailAddress, true);
            if (user is null)
            {
                response.Message = $"Could not find the account with e-mail address: {emailAddress}";
                return response;
            }

            var unprotectedUserPassword = await userSecurityService.UnprotectAsync(user.Password!);
            
            if (unprotectedUserPassword == newPassword)
            {
                response.Message = "The new password cannot be the same as the existing password.";
                return response;
            }

            var passwordCheck = await userValidationService.ValidatePasswordAsync(newPassword);
            if (!passwordCheck.isValid)
            {
                response.Message = $"<strong>For new password:</strong><br /> {passwordCheck.errorMessage}";
                return response;
            }

            passwordCheck = await userValidationService.ValidatePasswordAsync(newPassword);
            if (!passwordCheck.isValid)
            {
                response.Message = $"<strong>For new password repeated:</strong><br /> {passwordCheck.errorMessage}";
                return response;
            }

            request.EmailAddress = emailAddress;
            user.Password = request.NewPassword;
            
            var requestModel = user.Adapt<UserUpdateRequestModel>();
            requestModel.ProtectedId = await userSecurityService.ProtectAsync(user.Id.ToString());
            requestModel.Password = request.NewPassword;
            requestModel.IsPasswordUpdating = true;

            var updatedUser = await userCommandService.UpdateUserAsync(requestModel);
            if (updatedUser is null)
            { 
                response.Message = $"Could not find the updated account with e-mail address: {emailAddress}";
                return response;
            }
            response.Success = true;
            response.Message = "Your password was succesfully reset.";
            return response;
        }

        public async Task<ResetPasswordResponseModel> VerifyResetPassword(string emailAddress, string forgotPasswordToken)
        {
            try
            {
                forgotPasswordToken = await userSecurityService.UnprotectAsync(forgotPasswordToken);
                emailAddress = await userSecurityService.UnprotectAsync(emailAddress);
                var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(emailAddress, true);
                var response = await userQueryRepository.GetForgotPasswordTokenAsync(user!.Id);
                response.ForgotPasswordToken = await userSecurityService.UnprotectAsync(response.ForgotPasswordToken!);

                if (response.ForgotPasswordToken == forgotPasswordToken && DateTime.Now <= new DateTime(response.ExpirationTime))
                {
                    return new ResetPasswordResponseModel
                    {
                        Success = true,
                        Message = ""
                    };
                }

                return new ResetPasswordResponseModel
                {
                    Message = "Your reset password link is invalid or expired."
                };
            }
            catch (Exception)
            {
                return new ResetPasswordResponseModel
                {
                    Message = "An error occurred while verifying reset password link."
                };
            }
        }
    }
}
