using Azure.Messaging.ServiceBus;
using WhoOwesWho.CurrencyService.Services.ServiceBus.Resolvers;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.CurrencyService.Services.ServiceBus.Handlers
{
    public class CurrencyMessageHandler(IMessageResolverService resolver) : IServiceBusMessageHandler
    {
        public string QueueName => CurrencyQueues.CurrencyRequest;
        
        public async Task<object?> HandleAsync(ServiceBusReceivedMessage request)
        {
            var input = request.Body.ToObjectFromJson<CurrencyRequestModel>();
            return await resolver.GetCurrencyAsync(input!.ApiKey!, input?.Iso!);
        }
    }
}
