using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserCommandService
    {
        Task<UserModel?> CreateUserAsync(UserModel request, string host);
        Task<UserModel?> UpdateUserAsync(UserModel request, string token);
    }

    public class UserCommandService(
        IConfiguration configuration, 
        IUserNotificationService userNotificationService, 
        IUserValidationService userValidationService,
        IUserSecurityService userSecurityService,
        IUserQueryRepository userQueryRepository, 
        IUserMutationRepository userMutationRepository
        ) : ServiceBase(configuration), IUserCommandService
    {
        public async Task<UserModel?> CreateUserAsync(UserModel request, string host)
        {
            var user = await userMutationRepository.CreateUserAsync(request);
            if (user is null)
            {
                return await Task.FromResult(new UserModel()
                {
                    Success = false,
                    Message = "An error occurred while creating the user. Please, try again."
                });
            }
            await userNotificationService.SendAccountConfirmationMessage(user, host);
            return await userQueryRepository.GetSingleUserByEmailAddressAsync(user.EmailAddress, true);
        }

        public async Task<UserModel?> UpdateUserAsync(UserModel request, string token)
        {
            var validationResult = await userValidationService.VerifyUpdate(request, token);
            if (validationResult is { Success: false, NoAdmin: false })
            {
                return await Task.FromResult(new UserModel()
                {
                    Success = false,
                    Message = "The event you assigned to already has an administrator. If you want to change to being and administrator, the current administrator must uncheck and update before you can check and update."
                });
            }

            request.Id = Guid.Parse(await userSecurityService.UnprotectAsync(request.ProtectedId!));
            var userEntity = await userQueryRepository.GetSingleUserByIdAsync(request.Id, true);
            userEntity!.FullName = request.FullName;
            userEntity.MobilePhoneNumber = request.MobilePhoneNumber;
            userEntity.Admin = request.Admin;

            var response = await Task.FromResult(await userMutationRepository.UpdateUserAsync(userEntity));
            if (validationResult is { Success: true, NoAdmin: true })
            {
                return await Task.FromResult(new UserModel()
                {
                    Success = true,
                    Message = "The event running is now left with no administrator. This is indeed not recommended as event and payment edit, delete and settlement are not available as these can only be performed by an administrator."
                });
            }
            response!.Message = "Profile updated successfully.";
            return response;
        }
    }
}
