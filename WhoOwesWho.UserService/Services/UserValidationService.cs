using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Auxiliaries;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.Gateways;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserValidationService
    {
        Task<(bool isValid, string errorMessage)> ValidatePasswordAsync(string? password, bool encrypted = false);
        Task<(bool isValid, string errorMessage)> ValidateEmailAsync(string emailAddress, bool? shouldExist = false);
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
        public async Task<(bool isValid, string errorMessage)> ValidatePasswordAsync(string? password, bool encypted = false)
        {
            if (encypted)
            {
                password = await userSecurityService.UnprotectAsync(password!);
            }
            var errorMessage = string.Empty;
            var check = password!.Length >= int.Parse(AppSettings.PasswordLengthRequired)
                        && password.Count(char.IsUpper) >= int.Parse(AppSettings.PasswordUppercaseRequired)
                        && password.Count(char.IsDigit) >= int.Parse(AppSettings.PasswordDigitsRequired);

            if (!check)
            {
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    Constants.CredentialsErrorMessages.PasswordRequirements, AppSettings.PasswordLengthRequired,
                    AppSettings.PasswordUppercaseRequired, AppSettings.PasswordDigitsRequired);
            }

            return (check, errorMessage);
        }

        public async Task<(bool isValid, string errorMessage)> ValidateEmailAsync(string emailAddress, bool? shouldExist = null)
        {
            try
            {
                const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                if (!regex.IsMatch(emailAddress))
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
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                throw new ArgumentException("Email address argument was not provided.");
            }

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
                var evt = await userCacheRepository.GetActiveEventByIdAsync(request.EventId);
                
                if (evt!.Name is null)
                {
                    return await CreateResponse(true);
                }
                var eventUsers = await GetEventUsersAsync(evt);
                
                var id = await userSecurityService.UnprotectAsync(request.ProtectedId!);

                var existingAdmin = eventUsers.SingleOrDefault(u => u.Admin);
                if (existingAdmin is null)
                {
                    return await CreateResponse(true);
                }
                if (existingAdmin.Id == Guid.Parse(id))
                {
                    return await CreateResponse(true, true);
                }
                if (request.Admin)
                {
                    return await CreateResponse(false);
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
