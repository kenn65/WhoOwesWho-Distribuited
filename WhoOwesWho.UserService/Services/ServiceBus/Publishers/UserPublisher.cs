using Azure.Messaging.ServiceBus;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.UserService.Services.ServiceBus.Publishers
{
    public interface IUserPublisher
    {
        Task DispatchAsync(UserMessageRequestModel model);
    }
    public class UserPublisher(ServiceBusClient client) : IUserPublisher
    {
        private readonly ServiceBusSender _sender = client.CreateSender(ServiceBusTopics.MessagingTopics.AuthenticationUserDispatchRequest);

        public async Task DispatchAsync(UserMessageRequestModel model)
        {
            var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(model))
            {
                Subject = nameof(UserMessageRequestModel)
            };

            await _sender.SendMessageAsync(message);
        }
    }
}
