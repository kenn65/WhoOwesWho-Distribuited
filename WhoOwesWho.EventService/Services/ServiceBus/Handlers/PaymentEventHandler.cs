using Azure.Messaging.ServiceBus;
using WhoOwesWho.EventService.Services.ServiceBus.Handling;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.EventService.Services.ServiceBus.Handlers
{
    public class PaymentEventHandler(IMessageResolverService resolver) : IServiceBusMessageHandler
    {
        public string QueueName => EventQueues.PaymentEventRequest;

        public async Task<object?> HandleAsync(ServiceBusReceivedMessage request)
        {
            var input = request.Body.ToObjectFromJson<SbEventRequestModel>();
            return await resolver.GetEventAsync(input!.ApiKey, input.UserOrEventId!, input.Active);
        }
    }
}
