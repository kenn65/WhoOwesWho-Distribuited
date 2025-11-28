using Azure.Messaging.ServiceBus;
using WhoOwesWho.CurrencyService.Services.ServiceBus.Resolvers;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using WhoOwesWho.Models.Models.Base.ServiceBus.RequestModels.Base;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.CurrencyService.Services.ServiceBus.Handlers
{
    public class CurrenciesMessageHandler(IMessageResolverService resolver) : IServiceBusMessageHandler
    {
        public string QueueName => CurrencyQueues.CurrenciesRequest;
        
        public async Task<object?> HandleAsync(ServiceBusReceivedMessage request)
        {
            var input = request.Body.ToObjectFromJson<RequestModelBase>();
            return await resolver.GetCurrenciesAsync(input!.ApiKey!);
        }
    }
}
