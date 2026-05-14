using Mapster;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Publishers;
using WhoOwesWho.AuthorizationService.Settings;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthenticationNotificationService
    {
        Task<AuthenticationResponseModel> SendAuthenticationMessageAsync(AuthenticationRequestModel model);
    }

    public class AuthenticationNotificationService(IConfiguration configuration,
        IAuthorizationCacheRepository authorizationCacheRepository,
        IMessagingPublisher messagingPublisher
        ) : ServiceBase(configuration), IAuthenticationNotificationService
    {
        public async Task<AuthenticationResponseModel> SendAuthenticationMessageAsync(AuthenticationRequestModel request)
        {
            
                var response = new AuthenticationResponseModel
                {
                    Success = true,
                    Message = Constants.AuthenticationErrorMessages.AuthenticationCodeSent
                };
                return await SendEventMessageAsync(request, response);
        }

        private async Task<AuthenticationResponseModel> SendEventMessageAsync(AuthenticationRequestModel request, AuthenticationResponseModel response)
        {
                var user = await authorizationCacheRepository.GetUserAsync(request.EmailAddress!);
                if (!user!.EmailAddressVerified)
                {
                    throw new Exception(Constants.AuthenticationErrorMessages.NotVerified);
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
                response.Code = messagingRequest.Code;
                response.Success = true;
                response.Message = Constants.AuthenticationErrorMessages.AuthenticationCodeSent;
                return response;
        }

        private static async Task<string> CreateRandomAuthenticationCode()
        {
            var randomizer = new Random();
            return randomizer.Next(100000, 990000).ToString("D5");
        }
    }
}
