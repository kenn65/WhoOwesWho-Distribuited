using System.ComponentModel.DataAnnotations;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Encryption;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Messaging;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IValidationService
    {
        Task<bool> ValidateUserCredentialsAsync(string emailAddress, string password);
    }
    public class ValidationService(
        IConfiguration configuration,
        IUnprotectValueMessageSender unprotectValueMessageSender,
        IUserMessageSender userMessageSender) : ServiceBase(configuration), IValidationService
    {
        public async Task<bool> ValidateUserCredentialsAsync([Required] string emailAddress, [Required] string password)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Email or password argument was not provided");
            }

            var user = await userMessageSender.SendAsync(new UserRequestModel
            {
                IdOrEmailAddress = emailAddress!,
                IncludePassword = true
            });

            if (user == null)
            {
                return await Task.FromResult(false);
            }

            if (!user.EmailAddressVerified)
            {
                return await Task.FromResult(false);
            }

            var unprotectedUserPassword = await unprotectValueMessageSender.SendAsync(new UnprotectValueRequestModel
            {
                ApiKey = AppSettings.EncryptionMicroServiceApiKey,
                Text = user.Password!
            });
            
            return await Task.FromResult(password == unprotectedUserPassword);
        }
    }
}

