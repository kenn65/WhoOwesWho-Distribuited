using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Auxiliaries;
using WhoOwesWho.UserService.Models;
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
        IDataQueryService userSelectionService,
        IDataMutationService userModificationService,
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

            return await Task.FromResult((check, errorMessage));
        }

        public async Task<(bool isValid, string errorMessage)> ValidateEmailAsync(string emailAddress, bool? shouldExist = null)
        {
            try
            {
                const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                if (!regex.IsMatch(emailAddress))
                {
                    return await Task.FromResult((false, Constants.CredentialsErrorMessages.EmailAddressNotValid));
                }

                switch (shouldExist)
                {
                    case false:
                        {
                            var any = await ValidateEmailExists(emailAddress);
                            if (any.isValid)
                            {
                                return await Task.FromResult((false, EmailAlreadyExists: Constants.CredentialsErrorMessages.EmailAddressAlreadyExists));
                            }

                            break;
                        }
                    case true:
                        {
                            var any = await ValidateEmailExists(emailAddress);
                            if (!any.isValid)
                            {
                                return await Task.FromResult((false, Constants.CredentialsErrorMessages.EmailAdddressDoesNotExist));
                            }

                            break;
                        }
                }

                return await Task.FromResult((true, string.Empty));
            }
            catch
            {
                return await Task.FromResult((false, Constants.CredentialsErrorMessages.EmailAddressNotValid));
            }
        }

        public async Task<UserModel?> VerifyUserEmailAddress(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                throw new ArgumentException("Email address argument was not provided.");
            }

            var user = await userSelectionService.GetSingleUserByEmailAddressAsync(emailAddress, true);
            if (user == null)
            {
                return null;
            }

            user.EmailAddressVerified = true;


            return await userModificationService.UpdateUserAsync(user);
        }

        public async Task<UpdateUserVerificationModel> VerifyUpdate(UserModel request, string token)
        {
            try
            {

                var thisEvent = await eventGatewayService.GetUserEventAsync(request.ProtectedId!, token, true, true);
                var eventUsers =
                    (await eventGatewayService.GetEventUsersAsync(thisEvent.Id.ToString(), token, true, true))
                    .ToList();
                var id = await encryptionGatewayService.UnprotectAsync(request.ProtectedId!, true);

                var existingAdmin = eventUsers.SingleOrDefault(u => u.Admin);
                if (existingAdmin == null)
                {
                    return await await Task.FromResult(CreateResponse(true));
                }

                if (existingAdmin.Id == Guid.Parse(id))
                {
                    return await await Task.FromResult(CreateResponse(true, true));
                }
                
                return await await Task.FromResult(CreateResponse(false));
            }
            catch
            {
                return await await Task.FromResult(CreateResponse(false));
            }
        }

        private async Task<(bool isValid, string errorMessage)> ValidateEmailExists(string emailAddress)
        {
            try
            {
                await using var connection = new SqlConnection(AppSettings.DatabaseConnectionString);
                connection.Open();
                var command =
                    new SqlCommand(
                        $"SELECT COUNT([Id]) FROM [WoW.Users].[dbo].[WoW.User] WHERE [EmailAddress] = @emailAddress", connection);
                command.Parameters.AddWithValue("@emailAddress", emailAddress);
                var result = await command.ExecuteScalarAsync();
                if ((int)result! > 0)
                {
                    return await Task.FromResult((true, string.Empty));
                }
                return await Task.FromResult((false, Constants.CredentialsErrorMessages.EmailAddressAlreadyExists));
            }
            catch (SqlException e)
            {
                throw new Exception($"An error occurred while validating the email address: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while validating the email address: {e.Message}", e);
            }
        }

        private static async Task<UpdateUserVerificationModel> CreateResponse(bool success, bool noAdmin = false)
        {
            return await Task.FromResult(new UpdateUserVerificationModel
            {
                Success = success,
                NoAdmin = noAdmin
            });
        }
    }
}
