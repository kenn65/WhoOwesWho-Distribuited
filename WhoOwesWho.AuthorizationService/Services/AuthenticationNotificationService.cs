using Mapster;
using WhoOwesWho.AuthorizationService.Models;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Publishers;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthenticationNotificationService
    {
        Task<string> SendAuthenticationMessage(AuthenticationRequestModel model);
    }

    public class AuthenticationNotificationService(IConfiguration configuration,
        IAuthorizationCacheRepository authorizationCacheRepository,
        IMessagingPublisher messagingPublisher,
        IAuthorizationSecurityService authorizationSecurityService
        ) : ServiceBase(configuration), IAuthenticationNotificationService
    {
        public async Task<string> SendAuthenticationMessage(AuthenticationRequestModel request)
        {
            try
            {
                request.EmailAddress = await authorizationSecurityService.UnprotectAsync(request.EmailAddress!);
                var user = await authorizationCacheRepository.GetUserAsync(request.EmailAddress!);
                if (user is null)
                {
                    throw new ArgumentException($"User with e-mail address: {request.EmailAddress} was not found");
                }

                var entity = user.Adapt<UserMessageRequestModel>();

                var messagingRequest = new MessagingRequestModel
                {
                    ApiKey = AppSettings.MessagingMicroServiceApiKey,
                    Host = request.Host,
                    Type = "Authentication",
                    User = entity,
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
