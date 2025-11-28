using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.UserService.Services.ServiceBus.Senders.Messaging
{
    public interface IForgotPasswordMessageSender
    {
        Task<bool> SendAsync(MessagingRequestModel request);
    }
    public class ForgotPasswordMessageSender : EventServiceSenderBase, IForgotPasswordMessageSender
    {
        private readonly ServiceBusProcessor _processor;

        public ForgotPasswordMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(MessagingQueues.ForgotPasswordResponse);
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
                MessagingQueues.ForgotPasswordRequest,
                MessagingQueues.ForgotPasswordResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<bool>(args);
        }


    }
}
