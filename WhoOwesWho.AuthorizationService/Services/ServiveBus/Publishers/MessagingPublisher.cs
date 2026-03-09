using Azure.Messaging.ServiceBus;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.AuthorizationService.Services.ServiveBus.Publishers 
{ 
    public interface IMessagingPublisher
    {
        Task DispatchAsync(MessagingRequestModel model);
    }

    public class MessagingPublisher : IMessagingPublisher
    {
        private readonly ServiceBusSender _sender;

        public MessagingPublisher(ServiceBusClient client)
        {
            _sender = client.CreateSender(ServiceBusTopics.MessagingTopics.MessagingDispatchRequest);
        }

        public async Task DispatchAsync(MessagingRequestModel model)
        {
       
            var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(model))
            {
                Subject = nameof(MessagingRequestModel)
            };

            await _sender.SendMessageAsync(message);
        }
    }
}
