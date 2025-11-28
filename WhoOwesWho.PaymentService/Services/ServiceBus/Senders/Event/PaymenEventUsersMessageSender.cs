using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.UserService.Services.ServiceBus.Senders.Event
{
    public interface IPaymenEventUsersMessageSender
    {
        Task<IEnumerable<UserModel>> SendAsync(SbEventRequestModel request);
    }

    public class PaymenEventUsersMessageSender : EventServiceSenderBase, IPaymenEventUsersMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public PaymenEventUsersMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(EventQueues.PaymentEventUsersResponse);
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
                EventQueues.PaymentEventUsersRequest,
                EventQueues.PaymentEventUsersResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<IEnumerable<UserModel>>(args);
        }
    }
}
