using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserValidationService
    {
        Task<UserModel?> VerifyUserEmailAddressAsync(string emailAddress);
        Task<UpdateUserVerificationModel> ValidateUpdateAsync(UserUpdateRequestModel request);
        Task<bool> IsFullNameUniqueAsync(string fullName);
        Task<bool> DoesFullNameExistAsync(string fullName);
        Task<bool> IsEmailAddressUniqueAsync(string emailAddress);
        Task<bool> DoesEmailAddressExistAsync(string emailAddress);
    }
    public class UserValidationService(
        IConfiguration configuration,
        IUserQueryRepository userQueryRepository,
        IUserMutationRepository userMutationRepository,
        IUserUpdateValidationService userUpdateValidationService
        ) : ServiceBase(configuration), IUserValidationService
    {
        public async Task<UserModel?> VerifyUserEmailAddressAsync(string emailAddress)
        {
            var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(emailAddress, true);
            user?.EmailAddressVerified = true;
            return await userMutationRepository.UpdateUserAsync(user!);
        }

        public async Task<UpdateUserVerificationModel> ValidateUpdateAsync(UserUpdateRequestModel request)
        {
            return await userUpdateValidationService.ValidateUpdateAsync(request);
        }

        public async Task<bool> IsFullNameUniqueAsync(string fullName)
        {
            return !(await userQueryRepository.GetUserFullNameExists(fullName));
        }

        public async Task<bool> DoesFullNameExistAsync(string fullName)
        {
            return await userQueryRepository.GetUserFullNameExists(fullName);
        }

        public async Task<bool> IsEmailAddressUniqueAsync(string emailAddress)
        {
            return !(await userQueryRepository.GetUserEmailExists(emailAddress));
        }

        public async Task<bool> DoesEmailAddressExistAsync(string emailAddress)
        {
            return (await userQueryRepository.GetUserEmailExists(emailAddress));
        }
    }
}
