using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Currency
{ 
    public interface IPaymentExchangeRateMessageSender
    {
        Task<ExchangeRateResponseModel> SendAsync(ExchangeRateRequestModel request);
    }

    public class PaymentExchangeRateMessageSender : EventServiceSenderBase, IPaymentExchangeRateMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public PaymentExchangeRateMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(CurrencyQueues.ExchangeRateResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };

            _processor.StartProcessingAsync();
        }
        
        public async Task<ExchangeRateResponseModel> SendAsync(ExchangeRateRequestModel request)
        {
            var task = await SendRequestAsync<ExchangeRateRequestModel, ExchangeRateResponseModel>(
                request,
                CurrencyQueues.ExchangeRateRequest,
                CurrencyQueues.ExchangeRateResponse
                );

            return await task; 
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<ExchangeRateResponseModel>(args);
        }
    }
}
