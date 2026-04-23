using System.Globalization;
using System.Text.RegularExpressions;
using WhoOwesWho.Shared.Extensions;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Auxiliaries;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserValidationService
    {
        Task<(bool isValid, string errorMessage)> ValidatePasswordAsync(string? password);
        Task<(bool isValid, string errorMessage)> ValidateEmailAsync(string emailAddress, bool shouldExist);
        Task<UserModel?> VerifyUserEmailAddress(string emailAddress);
        Task<UpdateUserVerificationModel> VerifyUpdate(UserUpdateRequestModel request);
    }
    public class UserValidationService(
        IConfiguration configuration,
        IUserQueryRepository userQueryRepository,
        IUserMutationRepository userMutationRepository,
        IUserSecurityService userSecurityService,
        IUserCacheRepository userCacheRepository
        ) : ServiceBase(configuration), IUserValidationService
    {
        public async Task<(bool isValid, string errorMessage)> ValidatePasswordAsync(string? password)
        {
            var errorMessage = string.Empty;
            var isValidPassword = password!.IsValid(AppSettings.PasswordLengthRequired, AppSettings.PasswordUppercaseRequired, AppSettings.PasswordDigitsRequired);
            if (!isValidPassword)
            {
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    Constants.CredentialsErrorMessages.PasswordRequirements, AppSettings.PasswordLengthRequired,
                    AppSettings.PasswordUppercaseRequired, AppSettings.PasswordDigitsRequired);
            }
            return (isValidPassword, errorMessage);
        }

        public async Task<(bool isValid, string errorMessage)> ValidateEmailAsync(string emailAddress, bool shouldExist)
        {
            try
            {
                if (!emailAddress.IsValid())
                {
                    return (false, Constants.CredentialsErrorMessages.EmailAddressNotValid);
                }

                switch (shouldExist)
                {
                    case false:
                        {
                            var any = await ValidateEmailExists(emailAddress);
                            if (any.isValid)
                            {
                                return (false, Constants.CredentialsErrorMessages.EmailAddressAlreadyExists);
                            }
                            return (true, string.Empty);
                        }
                    case true:
                        {
                            var any = await ValidateEmailExists(emailAddress);
                            if (!any.isValid)
                            {
                               return (false, Constants.CredentialsErrorMessages.EmailAdddressDoesNotExist);
                            }
                            break;
                        }
                }

                return (true, string.Empty);
            }
            catch
            {
                return (false, Constants.CredentialsErrorMessages.EmailAddressNotValid);
            }
        }

        public async Task<UserModel?> VerifyUserEmailAddress(string emailAddress)
        {
            var user = await userQueryRepository.GetSingleUserByEmailAddressAsync(emailAddress, true);
            if (user is null)
            {
                return null;
            }
            user.EmailAddressVerified = true;
            return await userMutationRepository.UpdateUserAsync(user);
        }

        public async Task<UpdateUserVerificationModel> VerifyUpdate(UserUpdateRequestModel request)
        {
            try
            {
                if (request.EventId is null)
                {
                    return await CreateResponse(true);
                }
                request.EventId = await userSecurityService.UnprotectAsync(request.EventId);
                if (request.EventId == Guid.Empty.ToString())
                {
                    return await CreateResponse(true);
                }
                var evt = await userCacheRepository.GetActiveEventByIdAsync(request.EventId);
                var eventUsers = await GetEventUsersAsync(evt!);
                var id = await userSecurityService.UnprotectAsync(request.ProtectedId!);
                var existingAdmin = eventUsers.SingleOrDefault(u => u.Admin);
                if (existingAdmin is null && request.Admin)
                {
                    return await CreateResponse(true);
                }
                if (request.Admin && existingAdmin is not null && existingAdmin.Id != Guid.Parse(id))
                {
                    return await CreateResponse(false);
                }
                if (existingAdmin is null && !request.Admin)
                {
                    return await CreateResponse(true, true);
                }
                return await CreateResponse(true);
            }
            catch
            {
                return await CreateResponse(false);
            }
        }

        private async Task<(bool isValid, string errorMessage)> ValidateEmailExists(string emailAddress)
        {
            var check = await userQueryRepository.GetUserEmailExists(emailAddress);
            if (check)
            {
                return (true, string.Empty);
            }
            return (false, string.Empty);
        }

        private async Task<IEnumerable<UserMessageResponseModel>> GetEventUsersAsync(EventMessageResponseModel evt)
        {
            return (await Task.WhenAll(
                   evt.UserIds!.Select(async id =>
                   await userCacheRepository.GetUserByIdAsync(id.ToString())
                   ?? new UserMessageResponseModel()
                   ))).ToList();
        }

        private static async Task<UpdateUserVerificationModel> CreateResponse(bool success, bool noAdmin = false)
        {
            return new UpdateUserVerificationModel
            {
                Success = success,
                NoAdmin = noAdmin
            };
        }
    }
}
