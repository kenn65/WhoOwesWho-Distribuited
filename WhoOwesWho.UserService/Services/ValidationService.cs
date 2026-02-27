using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Auxiliaries;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.Gateways;

namespace WhoOwesWho.UserService.Services
{
    public interface IValidationService
    {
        Task<(bool isValid, string errorMessage)> ValidatePasswordAsync(string? password, bool encrypted = false);
        Task<(bool isValid, string errorMessage)> ValidateEmailAsync(string emailAddress, bool? shouldExist = false);
        Task<UserModel?> VerifyUserEmailAddress(string emailAddress);
        Task<UpdateUserVerificationModel> VerifyUpdate(UserModel request, string token);
    }
    public class ValidationService(
        IConfiguration configuration,
        IUserQueryRepository userQueryRepository,
        IUserMutationRepository userMutationRepository,
        IEncryptionGatewayService encryptionGatewayService,
        IEventGatewayService eventGatewayService
        ) : ServiceBase(configuration), IValidationService
    {
        public async Task<(bool isValid, string errorMessage)> ValidatePasswordAsync(string? password, bool encypted = false)
        {
            if (encypted)
            {
                password = await encryptionGatewayService.UnprotectAsync(password!, false);
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
            if (user == null)
            {
                return null;
            }

            user.EmailAddressVerified = true;


            return await userMutationRepository.UpdateUserAsync(user);
        }

        public async Task<UpdateUserVerificationModel> VerifyUpdate(UserModel request, string token)
        {
            try
            {
                var thisEvent = await eventGatewayService.GetUserEventAsync(request.ProtectedId!, token, true, true);
                if (thisEvent.Name == null)
                {
                    return await CreateResponse(true);
                }
                var eventUsers =
                    (await eventGatewayService.GetEventUsersAsync(thisEvent.Id.ToString(), token, true, true))
                    .ToList();
                var id = await encryptionGatewayService.UnprotectAsync(request.ProtectedId!, true);

                var existingAdmin = eventUsers.SingleOrDefault(u => u.Admin);
                if (existingAdmin == null)
                {
                    return await CreateResponse(true);
                }

                if (existingAdmin.Id == Guid.Parse(id))
                {
                    return await CreateResponse(true, true);
                }
                
                return await CreateResponse(false);
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
