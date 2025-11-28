using Azure.Messaging.ServiceBus;
using WhoOwesWho.EncryptionService.Services.ServiceBus.Resolvers;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.EncryptionService.Services.ServiceBus.Handlers
{
    public class PaymentProtectValueMessageHandler(IMessageResolverService resolver) : IServiceBusMessageHandler
    {
        public string QueueName => EncryptionQueues.PaymentProtectReuest;
        
        public async Task<object?> HandleAsync(ServiceBusReceivedMessage request)
        {
            var input = request.Body.ToObjectFromJson<ProtectValueRequestModel>();
            return (await resolver.ProtectAsync(input!.ApiKey, input.Text)).ProtectedValue;
        }
    }
}


