using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.User
{
    public interface IAuthenticationMessageSender
    {
        Task<bool> SendAsync(MessagingRequestModel request);
    }

    public class AuthenticationMessageSender : EventServiceSenderBase, IAuthenticationMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public AuthenticationMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(MessagingQueues.AuthenticationValidateResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };

            _processor.StartProcessingAsync();
        }

        public async Task<bool> SendAsync(MessagingRequestModel request)
        {
            var task = await SendRequestAsync<MessagingRequestModel, bool>(
                request,
                MessagingQueues.AuthenticationValidateRequest,
                MessagingQueues.AuthenticationValidateResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<bool>(args);
        }
    }
}
