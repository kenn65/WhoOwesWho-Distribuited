using Mapster;
using WhoOwesWho.Shared.Auxiliaries;
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
        IUserLookupService userLookupService,
        IUserQueryRepository userQueryRepository,
        IUserCommandService userCommandService,
        IUserSecurityService userSecurityService
        ) : ServiceBase(configuration), IResetPasswordService
    {
        public async Task<ResetPasswordResponseModel?> ResetPasswordAsync(ResetPasswordRequestModel request)
        {
            var user = await userLookupService.GetSingleUserByEmailAddressAsync(request.EmailAddress, true)
                ?? throw new Exception($"{Constants.ResetPasswordErrorMessages.UserAccountNotFound} {request.EmailAddress}");
            
            var unprotectedUserPassword = await userSecurityService.UnprotectAsync(user.Password!);

            if (unprotectedUserPassword == request.NewPassword)
            {
                return new ResetPasswordResponseModel
                {
                    Message = Constants.ResetPasswordErrorMessages.NewPasswordSameAsExisting
                };
            }
            
            user.Password = await userSecurityService.ProtectAsync(request.NewPassword!, true);
            var requestModel = user.Adapt<UserUpdateRequestModel>();
            requestModel.IsPasswordUpdating = true;

            var updatedUser = await userCommandService.UpdateUserAsync(requestModel) 
                ?? throw new Exception($"{Constants.ResetPasswordErrorMessages.UserAccountNotFound} {request.EmailAddress}");
            
            return new ResetPasswordResponseModel
            {
                Success = true,
                Message = Constants.ResetPasswordErrorMessages.ResetSucceeded
            };
        }

        public async Task<ResetPasswordResponseModel> VerifyResetPassword(string emailAddress, string forgotPasswordToken)
        {
            var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(emailAddress, true);
            var response = await userQueryRepository.GetForgotPasswordTokenAsync(user!.Id);
            response.ForgotPasswordToken = await userSecurityService.UnprotectAsync(response.ForgotPasswordToken!);

            if (response.ForgotPasswordToken == forgotPasswordToken && DateTime.Now <= new DateTime(response.ExpirationTime))
            {
                return new ResetPasswordResponseModel
                {
                    Success = true
                };

            }
            throw new Exception(Constants.ResetPasswordErrorMessages.ResetPasswordTokenInvalid);
        }
    }
}
