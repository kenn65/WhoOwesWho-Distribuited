using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Messaging
{
    public interface IUserMessageSender
    {
        Task<UserModel> SendAsync(UserRequestModel request);
    }

    public class UserMessageSender : EventServiceSenderBase, IUserMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public UserMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(UserQueues.AuthorizationUserResponse);
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
                UserQueues.AuthorizationUserRequest,
                UserQueues.AuthorizationUserResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<UserModel>(args);
        }
    }
}
