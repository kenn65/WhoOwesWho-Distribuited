using Mapster;
using WhoOwesWho.PaymentService.Models;
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
        IUserSecurityService userSecurityService,
        IUserQueryRepository userQueryRepository,
        IUserMutationRepository userMutationRepository,
        IUserPublishingServicee userPublishingServicee,
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
                    Message = "An error occurred while creating the user. Please, try again."
                };
            }

            var entity = user.Adapt<UserMessageRequestModel>();
            await userNotificationService.SendAccountConfirmationMessage(entity, host);
            return await userQueryRepository.GetSingleUserByEmailAddressAsync(user.EmailAddress, true);
        }

        public async Task<UserModel?> UpdateUserAsync(UserUpdateRequestModel? request)
        {
            var validationResult = request!.IsPasswordUpdating 
                ? new UpdateUserVerificationModel
                {
                    Success = true,
                    NoAdmin = true
                }
                : await userValidationService.VerifyUpdate(request!);

            if (validationResult is { Success: false, NoAdmin: false })
            {
                return new UserModel()
                {
                    Success = false,
                    Message = "The event you have assigned to already has an administrator."
                };
            }
                       
            request!.Id = Guid.Parse(await userSecurityService.UnprotectAsync(request!.ProtectedId!));
            var userEntity = await userQueryRepository.GetSingleUserByIdAsync(request!.Id, true);
            userEntity!.FullName = request.FullName;
            userEntity.MobilePhoneNumber = request.MobilePhoneNumber;
            userEntity.Admin = request.Admin;
            if (request.IsPasswordUpdating)
            {
                userEntity.Password = request.Password;
            }

            var response = await userMutationRepository.UpdateUserAsync(userEntity);
            if (validationResult is { Success: true, NoAdmin: true })
            {
                var user = response.Adapt<UserMessageRequestModel>();
                await userPublishingServicee.SendUserAsync(user);
                return new UserModel
                {
                    Success = true,
                    Message = "The event running is now left with no administrator. This is indeed not recommended as event and payment edit, delete and settlement are not available as these can only be performed by an administrator."
                };
            }
            response!.Success = true;
            response!.Message = "Profile updated successfully.";
            var entity = response.Adapt<UserMessageRequestModel>();
            await userPublishingServicee.SendUserAsync(entity);
            return response;
        }
    }
}
