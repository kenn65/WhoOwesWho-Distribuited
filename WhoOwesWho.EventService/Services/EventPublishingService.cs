using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.EventService.Services.ServiceBus.Publishers;
using WhoOwesWho.PaymentService.Models;

namespace WhoOwesWho.EventService.Services
{
    public interface IEventPublishingService
    {
        Task SendEventAsync(EventMessageRequestModel evt);
    }

    public class EventPublishingService(IConfiguration configuration, IEventPublisher eventPublisher) : ServiceBase(configuration), IEventPublishingService
    {
        public async Task SendEventAsync(EventMessageRequestModel thisEvent)
        {
            try
            {
                thisEvent.ApiKey = AppSettings.PaymentMicroServiceApiKey;
                await eventPublisher.DispatchAsync(thisEvent);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while sending the account confirmation message: {e.Message}",
                    e);
            }
        }
    }
}
