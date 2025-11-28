using Azure.Messaging.ServiceBus;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Event
{
    public interface IPaymentEventMessageSender 
    {
        Task<EventResponseModel> SendAsync(SbEventRequestModel request);
    }

    public class PaymentEventMessageSender : EventServiceSenderBase, IPaymentEventMessageSender
    {
        
        private readonly ServiceBusProcessor _processor;
        public PaymentEventMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(EventQueues.PaymentEventResponse);
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
                EventQueues.PaymentEventRequest,
                EventQueues.PaymentEventResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<EventResponseModel>(args);
        }
    }
}
