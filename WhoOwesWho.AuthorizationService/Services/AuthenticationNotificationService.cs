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
        Task<AuthenticationResponseModel> SendAuthenticationMessage(AuthenticationRequestModel model);
    }

    public class AuthenticationNotificationService(IConfiguration configuration,
        IAuthorizationCacheRepository authorizationCacheRepository,
        IMessagingPublisher messagingPublisher,
        IAuthorizationSecurityService authorizationSecurityService,
        IAuthenticationValidationService authenticationValidationService
        ) : ServiceBase(configuration), IAuthenticationNotificationService
    {
        public async Task<AuthenticationResponseModel> SendAuthenticationMessage(AuthenticationRequestModel request)
        {
            var response = new AuthenticationResponseModel();
            try
            {
                if (string.IsNullOrWhiteSpace(request.EmailAddress) || string.IsNullOrWhiteSpace(request.Password))
                {
                    response.Message = "E-mail address or password was not provided";
                    return response;
                }

                if (!await authenticationValidationService.ValidateUserCredentialsAsync(request.EmailAddress, request.Password))
                {
                    response.Message = "Invalid e-mail and/or password entered.";
                    return response;
                }

                request.EmailAddress = await authorizationSecurityService.UnprotectAsync(request.EmailAddress!);
                var user = await authorizationCacheRepository.GetUserAsync(request.EmailAddress!);
                if (user is null)
                {
                    response.Message =$"User with e-mail address: {request.EmailAddress} was not found";
                    return response;
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
                response.Message = "An authentication code was sent to your e-mail address";
                return response;
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
            return randomizer.Next(100000, 990000).ToString("D5");
        }
    }
}
