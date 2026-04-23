using Azure;
using Azure.Core;
using Mapster;
using WhoOwesWho.AuthorizationService.Repositories;
using WhoOwesWho.AuthorizationService.Services.Base;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Publishers;
using WhoOwesWho.AuthorizationService.Settings;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.AuthorizationService.Services
{
    public interface IAuthenticationNotificationService
    {
        Task<AuthenticationResponseModel> SendAuthenticationMessageAsync(AuthenticationRequestModel model);
    }

    public class AuthenticationNotificationService(IConfiguration configuration,
        IAuthorizationCacheRepository authorizationCacheRepository,
        IMessagingPublisher messagingPublisher,
        IAuthorizationSecurityService authorizationSecurityService,
        IAuthenticationValidationService authenticationValidationService
        ) : ServiceBase(configuration), IAuthenticationNotificationService
    {
        public async Task<AuthenticationResponseModel> SendAuthenticationMessageAsync(AuthenticationRequestModel request)
        {
            var response = new AuthenticationResponseModel();
            try
            {
                if (string.IsNullOrWhiteSpace(request.EmailAddress) || string.IsNullOrWhiteSpace(request.Password))
                {
                    response.Message = "E-mail address or password was not provided";
                    return response;
                }

                request.EmailAddress = await authorizationSecurityService.UnprotectAsync(request.EmailAddress!);
                var validationType = await authenticationValidationService.ValidateUserCredentialsAsync(request.EmailAddress, request.Password);
                switch (validationType)
                {
                    case AuthenticationValidationTypes.UserCredentialsInvalid:
                        response.Message = "Invalid e-mail and/or password entered.";
                        return response;
                    case AuthenticationValidationTypes.UserInvalid:
                        response.Message = $"User with e-mail address: {request.EmailAddress} was not found";
                        return response;
                    case AuthenticationValidationTypes.EmailAddressVerificationInvalid:
                        response.Message = $"E-mail address: {request.EmailAddress} is not verified. Please verify your e-mail address by the membership e-mail sent to you upon signing up.";
                        return response;
                    case AuthenticationValidationTypes.UserCredentialsValid:
                        return await SendEventMessageAsync(request, response);
                }
                response.Message = "An error occurred while validating the user credentials. Please try again later.";
                return response;
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while sending the account confirmation message: {e.Message}",
                    e);
            }
        }

        private async Task<AuthenticationResponseModel> SendEventMessageAsync(AuthenticationRequestModel request, AuthenticationResponseModel response)
        {
            var user = await authorizationCacheRepository.GetUserAsync(request.EmailAddress!);
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

        private static async Task<string> CreateRandomAuthenticationCode()
        {
            var randomizer = new Random();
            return randomizer.Next(100000, 990000).ToString("D5");
        }
    }
}
