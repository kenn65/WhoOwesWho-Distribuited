using Azure.Messaging.ServiceBus;
using WhoOwesWho.EncryptionService.Services.ServiceBus.Resolvers;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.EncryptionService.Services.ServiceBus.Handlers
{
    public class PaymentUnprotectValueMessageHandler(IMessageResolverService resolver) : IServiceBusMessageHandler
    {
        public string QueueName => EncryptionQueues.PaymentUnprotectRequest;
        
        public async Task<object?> HandleAsync(ServiceBusReceivedMessage request)
        {
            var input = request.Body.ToObjectFromJson<UnprotectValueRequestModel>();
            return (await resolver.UnprotectAsync(input!.ApiKey, input.Text)).UnprotectedValue;
        }
    }
}
