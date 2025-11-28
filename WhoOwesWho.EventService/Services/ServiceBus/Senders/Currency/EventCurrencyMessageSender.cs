using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.EventService.Services.ServiceBus.Senders.Currency
{
    public interface IEventCurrencyMessageSender
    {
        Task<CurrencyResponseModel> SendAsync(string apiKey, string iso);
    }

    public class EventCurrencyMessageSender : EventServiceSenderBase, IEventCurrencyMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public EventCurrencyMessageSender(ServiceBusClient client)
            : base(client)
        {
            // Create processor for the response queue
            _processor = client.CreateProcessor(CurrencyQueues.CurrencyResponse);

            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };

            _processor.StartProcessingAsync();
        }

        public async Task<CurrencyResponseModel> SendAsync(string apikey, string iso)
        {
            var request = new CurrencyRequestModel 
            { 
                Iso = iso,
                ApiKey = apikey
            };

            // This returns a Task<CurrencyModel>
            var task = await SendRequestAsync<CurrencyRequestModel, CurrencyResponseModel>(
                request,
                CurrencyQueues.CurrencyRequest,
                CurrencyQueues.CurrencyResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<CurrencyResponseModel>(args);
        }
    }
}
