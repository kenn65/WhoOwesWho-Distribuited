using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Encryption
{
    public interface IUnprotectValueMessageSender
    {
        Task<string> SendAsync(string text);
    }

    public class UnprotectValueMessageSender : EventServiceSenderBase, IUnprotectValueMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public UnprotectValueMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(EncryptionQueues.UserUnprotectResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };

            _processor.StartProcessingAsync();
        }

        public async Task<string> SendAsync(string text)
        {
            var task = await SendRequestAsync<string, string>(
                text,
                EncryptionQueues.UserUnprotectRequest,
                EncryptionQueues.UserUnprotectResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<string>(args);
        }
    }
}
