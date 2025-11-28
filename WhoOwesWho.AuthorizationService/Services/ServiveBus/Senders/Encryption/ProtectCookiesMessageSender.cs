using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Encryption
{
    public interface IProtectCookiesMessageSender
    {
        Task<EncryptedCookiesResponseModel> SendAsync(CookiesRequestModel request);
    }
    
    public class ProtectCookiesMessageSender : EventServiceSenderBase, IProtectCookiesMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public ProtectCookiesMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(EncryptionQueues.AuthorizationProtectCookiesResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };
            _processor.StartProcessingAsync();
        }

        public async Task<EncryptedCookiesResponseModel> SendAsync(CookiesRequestModel request)
        {
            var task = await SendRequestAsync<CookiesRequestModel, EncryptedCookiesResponseModel>(
                request,
                EncryptionQueues.AuthorizationProtectCookiesRequest,
                EncryptionQueues.AuthorizationProtectCookiesResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<EncryptedCookiesResponseModel>(args);
        }
    }
}
