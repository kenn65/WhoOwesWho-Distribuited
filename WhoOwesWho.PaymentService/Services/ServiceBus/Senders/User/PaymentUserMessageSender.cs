using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.EventService.Services.ServiceBus.Senders.User
{
    public interface IPaymentUserMessageSender
    {
        Task<UserModel> SendAsync(UserRequestModel request);
    }
    public class PaymentUserMessageSender : EventServiceSenderBase, IPaymentUserMessageSender
    {
                private readonly ServiceBusProcessor _processor;

        public PaymentUserMessageSender(ServiceBusClient client) : base(client)
        {
            _processor = client.CreateProcessor(UserQueues.PaymentUserResponse);
            _processor.ProcessMessageAsync += ProcessResponseMessageAsync;
            _processor.ProcessErrorAsync += async args =>
            {
                Console.WriteLine("Receiver error: " + args.Exception);
            };
            _processor.StartProcessingAsync();
        }

        public async Task<UserModel> SendAsync(UserRequestModel request)
        {
            var task = await SendRequestAsync<UserRequestModel, UserModel>(
                request,
                UserQueues.PaymentUserRequest,
                UserQueues.PaymentUserResponse);

            return await task;
        }

        private async Task ProcessResponseMessageAsync(ProcessMessageEventArgs args)
        {
            await HandleResponseAsync<UserModel>(args);
        }
    }
}
