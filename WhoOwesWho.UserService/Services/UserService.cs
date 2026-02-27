using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.Gateways;
using WhoOwesWho.UserService.Services.ServiceBus.Publishers;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserService
    {
        Task<UserModel?> UpdateUserAsync(UserModel request, string token);
        Task<UserModel?> CreateUserAsync(UserModel request, string host);
        Task<UserModel?> GetSingleUserByEmailAddressAsync(string? emailAddress, bool complete = false);
        Task<UserModel?> GetSingleUserByIdAsync(Guid id, bool complete = false);
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        Task<ForgotPasswordTokenModel> GetForgotPasswordTokenAsync(Guid userId);
    }
    public class UserService(
        IConfiguration configuration,
        IUserMutationRepository userMutationRepository,
        IUserQueryRepository userQueryRepository,
        IValidationService validationService,
        IEncryptionGatewayService encryptionGatewayService,
        IMessagingPublisher messagingPublisher
        ) : ServiceBase(configuration), IUserService
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
            await SendAccountConfirmationMessage(user, host);
            return await userQueryRepository.GetSingleUserByEmailAddressAsync(user.EmailAddress, true);
        }

        public async Task<UserModel?> UpdateUserAsync(UserModel request, string token)
        {
            var validationResult = await validationService.VerifyUpdate(request, token);
            if (validationResult is { Success: false, NoAdmin: false })
            {
                return await Task.FromResult(new UserModel()
                {
                    Success = false,
                    Message = "The event you assigned to already has an administrator. If you want to change to being and administrator, the current administrator must uncheck and update before you can check and update."
                });
            }

            request.Id = Guid.Parse(await encryptionGatewayService.UnprotectAsync(request.ProtectedId!, true));
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

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            return await userQueryRepository.GetAllUsersAsync();
        }

        public async Task<ForgotPasswordTokenModel> GetForgotPasswordTokenAsync(Guid userId)
        {
            return await userQueryRepository.GetForgotPasswordTokenAsync(userId);
        }

        public async Task<UserModel?> GetSingleUserByEmailAddressAsync(string? emailAddress, bool complete = false)
        {
            return await userQueryRepository.GetSingleUserByEmailAddressAsync(emailAddress, complete);
        }

        public async Task<UserModel?> GetSingleUserByIdAsync(Guid id, bool complete = false)
        {
            return await userQueryRepository.GetSingleUserByIdAsync(id, complete);
        }

        private async Task SendAccountConfirmationMessage(UserModel entity, string host)
        {
            try
            {
                var request = new MessagingRequestModel
                {
                    ApiKey = AppSettings.MessagingMicroServiceApiKey,
                    Host = host,
                    Type = "SignUp",
                    User = entity
                };

                await messagingPublisher.DispatchAsync(request);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while sending the account confirmation message: {e.Message}",
                    e);
            }

        }





    }
}
