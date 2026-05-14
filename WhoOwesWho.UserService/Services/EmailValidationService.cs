using System.Text.RegularExpressions;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Repositories;

namespace WhoOwesWho.UserService.Services
{
    public interface IEmailValidationService
    {
        Task<EmailValidationResponse> ValidateEmailAsync(string emailAddress, bool shouldExist);
    }

    public class EmailValidationService(IUserQueryRepository userQueryRepository) : IEmailValidationService
    {
        public async Task<EmailValidationResponse> ValidateEmailAsync(string emailAddress, bool shouldExist)
        {
            var response = new EmailValidationResponse();

            if (!await ValidateEmailformatAsync(emailAddress))
            {
                response.Message = Constants.CredentialsErrorMessages.EmailAddressInvalid;
            }
            switch (shouldExist)
            {
                case false:
                    {
                        var any = await ValidateEmailExists(emailAddress);
                        if (any)
                        {
                            response.Message = Constants.CredentialsErrorMessages.EmailAddressAlreadyExists;
                        }
                        else
                        {
                            response.Success = true;
                        }
                        break;
                    }
                case true:
                    {
                        var any = await ValidateEmailExists(emailAddress);
                        if (!any)
                        {
                            response.Message = Constants.CredentialsErrorMessages.EmailAdddressDoesNotExist;
                        }
                        else
                        {
                            response.Success = true;
                        }
                        break;
                    }
            }
            return response;
        }

        private async Task<bool> ValidateEmailExists(string emailAddress)
        {
            return await userQueryRepository.GetUserEmailExists(emailAddress);
        }

        private async Task<bool> ValidateEmailformatAsync(string emailAddress)
        {
            const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(emailAddress);

        }
    }
}