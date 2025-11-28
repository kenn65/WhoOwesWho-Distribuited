using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.EventService.Services.ServiceBus.Senders.User
{
    public interface IEventUserMessageSender
    {
        Task<UserModel> SendAsync(UserRequestModel request);
        
    }
    public class EventUserMessageSender : EventServiceSenderBase, IEventUserMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public EventUserMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(UserQueues.EventUserResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };
            _processor.StartProcessingAsync();
        }

        public async Task<UserModel> SendAsync(UserRequestModel request)
        {
            var task = await SendRequestAsync<UserRequestModel, UserModel>(
                request,
                UserQueues.EventUserRequest,
                UserQueues.EventUserResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<UserModel>(args);
        }

    }
}
