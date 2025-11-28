using Azure.Messaging.ServiceBus;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.UserService.Services.ServiceBus.Senders.Event
{
    public interface IUserEventUsersMessageSender
    {
        Task<IEnumerable<UserModel>> SendAsync(SbEventRequestModel request);
    }

    public class UserEventUsersMessageSender : EventServiceSenderBase, IUserEventUsersMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public UserEventUsersMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(EventQueues.UserEventUsersResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };

            _processor.StartProcessingAsync();
        }

        public async Task<IEnumerable<UserModel>> SendAsync(SbEventRequestModel request)
        {
            var task = await SendRequestAsync<SbEventRequestModel, IEnumerable<UserModel>>(
                request,
                EventQueues.UserEventUsersRequest,
                EventQueues.UserEventUsersResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<IEnumerable<UserModel>>(args);
        }
    }
}
