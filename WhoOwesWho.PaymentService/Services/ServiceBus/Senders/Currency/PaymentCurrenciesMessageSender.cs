using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using WhoOwesWho.Models.Models.Base.ServiceBus.RequestModels.Base;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Currency
{
    public interface IPaymentCurrenciesMessageSender
    {
        Task<IEnumerable<CurrencyResponseModel>> SendAsync(RequestModelBase request);
    }

    public class PaymentCurrenciesMessageSender : EventServiceSenderBase, IPaymentCurrenciesMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public PaymentCurrenciesMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(CurrencyQueues.CurrenciesResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };

            _processor.StartProcessingAsync();
        }

        public async Task<IEnumerable<CurrencyResponseModel>> SendAsync(RequestModelBase request)
        {
            var task = await SendRequestAsync<object?, IEnumerable<CurrencyResponseModel>>(
                request,
                CurrencyQueues.CurrenciesRequest,
                CurrencyQueues.CurrencyResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<IEnumerable<CurrencyResponseModel>>(args);
        }


    }
}
