using Azure.Messaging.ServiceBus;
using WhoOwesWho.MessagingService.Services.ServiceBus.Handling;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.MessagingService.Services.ServiceBus.Handlers
{
    public class SignUpHandler(IMessageResolverService resolver) : IServiceBusMessageHandler
    {
        public string QueueName => MessagingQueues.SignUpRequest;

        public async Task<object?> HandleAsync(ServiceBusReceivedMessage request)
        {
            var input = request.Body.ToObjectFromJson<MessagingRequestModel>();
            return await resolver.SendEmailAsync(input!);

        }
    }
}
