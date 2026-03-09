using Azure.Messaging.ServiceBus;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.EventService.Services.ServiceBus.Publishers
{
    public interface IEventPublisher
    {
        Task DispatchAsync(EventMessageRequestModel model);
    }

    public class EventPublisher(ServiceBusClient client) : IEventPublisher
    {
        private readonly ServiceBusSender _sender = client.CreateSender(ServiceBusTopics.MessagingTopics.PaymentEventDispatchRequest);

        public async Task DispatchAsync(EventMessageRequestModel model)
        {
            var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(model))
            {
                Subject = nameof(EventMessageRequestModel)
            };
            await _sender.SendMessageAsync(message);
        }
    }
}
