using Azure.Messaging.ServiceBus;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.UserService.Services.ServiceBus.Senders.Event
{
    public interface IUserEventMessageSender
    {
        Task<EventResponseModel> SendAsync(SbEventRequestModel request);
    }

    public class UserEventMessageSender : EventServiceSenderBase, IUserEventMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public UserEventMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(EventQueues.UserEventResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };
            _processor.StartProcessingAsync();
        }

        public async Task<EventResponseModel> SendAsync(SbEventRequestModel request)
        {
            var task = await SendRequestAsync<SbEventRequestModel, EventResponseModel>(
                request,
                EventQueues.UserEventRequest,
                EventQueues.UserEventResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<IEnumerable<UserModel>>(args);
        }
    }
}
