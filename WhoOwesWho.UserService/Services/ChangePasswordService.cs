using Mapster;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.Shared.Auxiliaries;

namespace WhoOwesWho.UserService.Services
{
    public interface IChangePasswordService
    {
        Task<ChangePasswordResponseModel?> ChangePasswordAsync(ChangePasswordRequestModel request);
    }
    public class ChangePasswordService(
        IConfiguration configuration,
        IUserSecurityService userSecurityService,
        IUserQueryRepository userQueryRepository,
        IUserCommandService userCommandService
        ) : ServiceBase(configuration), IChangePasswordService
    {
        public async Task<ChangePasswordResponseModel?> ChangePasswordAsync(ChangePasswordRequestModel request)
        {

            var response = new ChangePasswordResponseModel();

            if (request.NewPassword1 != request.NewPassword2)
            {
                throw new Exception(Constants.ChangePasswordErrorMessages.NewPasswordsDoNotMatch);
            }

            var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(request.EmailAddress, true);
            if (user is null)
            {
                throw new Exception(Constants.ChangePasswordErrorMessages.UserNotFound);
            }

            if ((await userSecurityService.UnprotectAsync(user.Password!)) != request.Password)
            {
                throw new Exception(Constants.ChangePasswordErrorMessages.ExistingPasswordInvalid);
            }

            user.Password = await userSecurityService.ProtectAsync(request.NewPassword1!, true);
            var requestModel = user.Adapt<UserUpdateRequestModel>();
            requestModel.IsPasswordUpdating = true;
            var userModel = await userCommandService.UpdateUserAsync(requestModel);
            if (userModel != null)
            {
                response.Success = true;
                response.Message = Constants.ChangePasswordErrorMessages.SuccessfullyChanged;
                return response;
            }
            throw new Exception(Constants.UserCreationErrorMessages.UserLoadingUnsuccessful);
        }
    }
}
