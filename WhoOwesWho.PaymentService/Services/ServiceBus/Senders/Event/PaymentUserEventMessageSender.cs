using Azure.Messaging.ServiceBus;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.UserService.Services.ServiceBus.Senders.Event
{
    public interface IPaymentUserEventMessageSender
    {
        Task<EventResponseModel> SendAsync(SbEventRequestModel request);
    }

    public class PaymentUserEventMessageSender : EventServiceSenderBase, IPaymentUserEventMessageSender
    {
        private readonly ServiceBusProcessor _processor;
        public PaymentUserEventMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(EventQueues.PaymentUserEventResponse);
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
                EventQueues.PaymentUserEventRequest,
                EventQueues.PaymentUserEventResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<EventResponseModel>(args);
        }
    }
}
