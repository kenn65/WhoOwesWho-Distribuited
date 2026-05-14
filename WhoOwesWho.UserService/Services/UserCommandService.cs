using Mapster;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserCommandService
    {
        Task<UserModel?> CreateUserAsync(UserModel request, string host);
        Task<UserModel?> UpdateUserAsync(UserUpdateRequestModel request);
    }

    public class UserCommandService(
        IConfiguration configuration,
        IUserNotificationService userNotificationService,
        IUserQueryRepository userQueryRepository,
        IUserMutationRepository userMutationRepository,
        IUserPublishingService userPublishingServicee,
        IUserValidationService userValidationService
        ) : ServiceBase(configuration), IUserCommandService
    {
        public async Task<UserModel?> CreateUserAsync(UserModel request, string host)
        {
            var user = await userMutationRepository.CreateUserAsync(request);
            if (user is null)
            {
                return new UserModel()
                {
                    Success = false,
                    Message = Constants.UserCreationErrorMessages.UserLoadingUnsuccessful
                };
            }
            var entity = user.Adapt<UserMessageRequestModel>();
            await userPublishingServicee.SendUserAsync(entity);
            await userNotificationService.SendAccountConfirmationMessage(entity, host);
            return await userQueryRepository.GetSingleUserByEmailAddressAsync(user.EmailAddress, true);
        }

        public async Task<UserModel?> UpdateUserAsync(UserUpdateRequestModel request)
        {
            var user = await userQueryRepository.GetSingleUserByIdAsync(request.Id);
            var validationResult = request!.IsPasswordUpdating
                ? new UpdateUserVerificationModel
                {
                    Success = true,
                    AdministratorNonExisting = true
                }
                : await userValidationService.ValidateUpdateAsync(request!);

            UserModel userModel = new UserModel();

            if (validationResult is { Success: false, AdministratorNonExisting: true })
            {
                userModel.Message = validationResult.Message;
            }
            if (validationResult is { Success: false, AdministratorNonExisting: false })
            {
                userModel.Message = validationResult.Message;
                return userModel;
            }

            var userEntity = await userQueryRepository.GetSingleUserByIdAsync(request!.Id, true);
            userEntity!.FullName = request.FullName;
            userEntity.MobilePhoneNumber = request.MobilePhoneNumber;
            userEntity.Admin = request.Admin;

            if (request.IsPasswordUpdating)
            {
                userEntity.Password = request.Password;
            }

            var response = await userMutationRepository.UpdateUserAsync(userEntity);
            var userEventModel = response.Adapt<UserMessageRequestModel>();
            await userPublishingServicee.SendUserAsync(userEventModel!);
            
            response!.Success = string.IsNullOrWhiteSpace(userModel.Message) 
                ? true 
                : false;

            response!.Message = string.IsNullOrWhiteSpace(userModel.Message)
                ? Constants.UserUpdatingErrorMessages.UpdateSucceeded
                : userModel.Message;

            // dispatch user
            var entity = response.Adapt<UserMessageRequestModel>();
            await userPublishingServicee.SendUserAsync(entity);

            return response;
        }
    }
}
