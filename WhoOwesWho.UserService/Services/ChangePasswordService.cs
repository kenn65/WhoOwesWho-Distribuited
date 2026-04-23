using Mapster;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IChangePasswordService
    {
        Task<ChangePasswordResponseModel?> ChangePasswordAsync(ChangePasswordRequestModel request);
    }
    public class ChangePasswordService(
        IConfiguration configuration,
        IUserSecurityService userSecurityService,
        IUserValidationService userValidationService,
        IUserQueryRepository userQueryRepository,
        IUserCommandService userCommandService
        ) : ServiceBase(configuration), IChangePasswordService
    {
        public async Task<ChangePasswordResponseModel?> ChangePasswordAsync(ChangePasswordRequestModel request)
        {
            var response = new ChangePasswordResponseModel();

            request.EmailAddress = await userSecurityService.UnprotectAsync(request.EmailAddress!);
            var password = await userSecurityService.UnprotectAsync(request.Password!);
            var newPassword1 = await userSecurityService.UnprotectAsync(request.NewPassword1!);
            var newPassword2 = await userSecurityService.UnprotectAsync(request.NewPassword2!);

            var emailCheck = await userValidationService.ValidateEmailAsync(request.EmailAddress!, true);
            if (!emailCheck.isValid)
            {
                response.Message = emailCheck.errorMessage;
                return response;
            }

            var passwordCheck = await userValidationService.ValidatePasswordAsync(password);
            if (!passwordCheck.isValid)
            {
                response.Message = $"For existing password:{passwordCheck.errorMessage}";
                return response;
            }

            passwordCheck = await userValidationService.ValidatePasswordAsync(newPassword1);
            if (!passwordCheck.isValid)
            {
                response.Message = $"For new password:{passwordCheck.errorMessage}";
                return response;
            }

            passwordCheck = await userValidationService.ValidatePasswordAsync(newPassword2!);
            if (!passwordCheck.isValid)
            {
                response.Message = $"For new password repeated:{passwordCheck.errorMessage}";
                return response;
            }

            if (newPassword1 != newPassword2)
            {
                response.Message = "The passwords does not match!";
                return response;
            }
            
            var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(request.EmailAddress, true);
            if (user is null)
            {
                response.Message = "User not found with the provided email address. Please try again";
                return response;
            }

           if (user.Password != request.Password)
            {
                response.Message = "The existing password is incorrect.";
                return response;
            }

            user.Password = request.NewPassword1;
            var requestModel = user.Adapt<UserUpdateRequestModel>();
            requestModel.ProtectedId = await userSecurityService.ProtectAsync(user.Id.ToString());
            requestModel.IsPasswordUpdating = true;
            var entity = await userCommandService.UpdateUserAsync(requestModel);
            if (entity != null)
            {
                response.Success = true;
                response.Message = "Your password was successfully changed.";
                return response;
            }

            response.Message = "An error occurred while updating the user password. Please try again";
            return response;
        }
    }
}
