using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.Gateways;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Publishers;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthenticationNotificationService
    {
        Task<string> SendAuthenticationMessage(AuthenticationRequestModel model);
    }

    public class AuthenticationNotificationService(IConfiguration configuration,
        IUserGatewayService userGatewayService,
        IMessagingPublisher messagingPublisher
        ) : ServiceBase(configuration), IAuthenticationNotificationService
    {
        public async Task<string> SendAuthenticationMessage(AuthenticationRequestModel request)
        {
            var user = await userGatewayService.GetUserAsync(request.EmailAddress!, false);
            if (user is null)
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

                await messagingPublisher.DispatchAsync(messagingRequest);
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
