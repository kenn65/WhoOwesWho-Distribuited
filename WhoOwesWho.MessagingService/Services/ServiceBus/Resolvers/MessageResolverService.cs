using System.ComponentModel.DataAnnotations;
using WhoOwesWho.Models.Models;


namespace WhoOwesWho.MessagingService.Services.ServiceBus.Handling
{
    public interface IMessageResolverService
    {
        Task<bool> SendEmailAsync([Required] MessagingRequestModel request);
    }

    public class MessageResolverService(ISecurityService securityService, IEmailMessagingService messagingService) : IMessageResolverService
    {
        public async Task<bool> SendEmailAsync([Required] MessagingRequestModel request)
        {
            if (!await securityService.ValidateApiKey(request.ApiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key");
            }
            return await messagingService.SendEmailAsync(request);
        }
    }
}
