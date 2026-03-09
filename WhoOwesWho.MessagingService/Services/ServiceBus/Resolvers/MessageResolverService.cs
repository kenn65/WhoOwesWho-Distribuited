using System.ComponentModel.DataAnnotations;
using WhoOwesWho.Shared.Models;


namespace WhoOwesWho.MessagingService.Services.ServiceBus.Handling
{
    public interface IMessageResolverService
    {
        Task<bool> SendEmailAsync([Required] MessagingRequestModel request);
    }

    public class MessageResolverService(IEmailMessagingService messagingService, IMessagingSecurityService messagingSecurityService) : IMessageResolverService
    {
        public async Task<bool> SendEmailAsync([Required] MessagingRequestModel request)
        {
            if (!await messagingSecurityService.ValidateApiKey(request.ApiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key");
            }
            return await messagingService.SendEmailAsync(request);
        }
    }
}
