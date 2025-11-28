using Azure.Messaging.ServiceBus;
using System.Text.Json;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.EventService.Services.ServiceBus.Senders.Encryption
{
    public interface IProtectValueMessageSender
    {
        Task<string> SendAsync(ProtectValueRequestModel request);
    }

    public class ProtectValueMessageSender : EventServiceSenderBase, IProtectValueMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public ProtectValueMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(EncryptionQueues.EventProtectResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };

            _processor.StartProcessingAsync();
        }

        public async Task<string> SendAsync(ProtectValueRequestModel request)
        {
            var task = await SendRequestAsync<ProtectValueRequestModel, string>(
                request,
                EncryptionQueues.EventProtectRequest,
                EncryptionQueues.EventProtectResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<string>(args);
        }
    }
}
