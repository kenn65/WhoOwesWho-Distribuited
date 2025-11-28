using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Encryption
{
    public interface IUnprotectValueMessageSender
    {
        Task<string> SendAsync(UnprotectValueRequestModel request);
    }

    public class UnprotectValueMessageSender : EventServiceSenderBase, IUnprotectValueMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public UnprotectValueMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(EncryptionQueues.AuthorizatonUnprotectResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };

            _processor.StartProcessingAsync();
        }

        public async Task<string> SendAsync(UnprotectValueRequestModel request)
        {
            var task = await SendRequestAsync<UnprotectValueRequestModel, string>(
                request,
                EncryptionQueues.AuthorizationUnprotectRequest,
                EncryptionQueues.AuthorizatonUnprotectResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<string>(args);
        }
    }
}
