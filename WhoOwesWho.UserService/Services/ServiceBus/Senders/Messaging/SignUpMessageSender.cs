using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.UserService.Services.ServiceBus.Senders.Messaging
{
    public interface ISignUpMessageSender
    {
        Task<bool> SendAsync(MessagingRequestModel request);
    }

    public class SignUpMessageSender: EventServiceSenderBase, ISignUpMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public SignUpMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(MessagingQueues.SignUpResponse);
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
                MessagingQueues.SignUpRequest,
                MessagingQueues.SignUpResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<bool>(args);
        }
    }
}
