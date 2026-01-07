using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Messaging;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.User;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthenticationService
    {
        Task<string> SendAuthenticationMessage(AuthenticationRequestModel model);
    }
    public class AuthenticationService(
        IConfiguration configuration, 
        IUserMessageSender userMessageSender, 
        IAuthenticationMessageSender authenticationMessageSender) : ServiceBase(configuration), IAuthenticationService
    {
        public async Task<string> SendAuthenticationMessage(AuthenticationRequestModel request)
        {
            var user = await userMessageSender.SendAsync(new UserRequestModel
            {
                ApiKey = AppSettings.UserMicroServiceApiKey,
                IdOrEmailAddress = request.EmailAddress!,
                IncludePassword = false
            });
                       
            if (user == null)
            {
                throw new ArgumentException($"User with e-mail address: {request.EmailAddress} was not found");
            }
            
            try
            {
                var messagingRequest = new MessagingRequestModel
                {
                    ApiKey = AppSettings.MessagingMicroServiceApiKey,
                    Host = request.Host,
                    Type = "Authentication",
                    User = user,
                    Code = await CreateRandomAuthenticationCode()
                };

                var result = await authenticationMessageSender.SendAsync(messagingRequest);
                
                if (result == false)
                {
                    throw new Exception("Failed to send authentication message");
                }
               
                return await Task.FromResult(messagingRequest.Code);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while sending the account confirmation message: {e.Message}",
                    e);
            }
        }

        private static async Task<string> CreateRandomAuthenticationCode()
        {
            var randomizer = new Random();
            return await Task.FromResult(randomizer.Next(100000, 990000).ToString("D5"));
        } 
    }
}
