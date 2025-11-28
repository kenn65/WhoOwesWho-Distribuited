using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using WhoOwesWho.UserService.Services.ServiceBus.Resolvers;
using static WhoOwesWho.Models.Models.Base.Queues;

namespace WhoOwesWho.UserService.Services.ServiceBus.Handlers
{
    public class PaymentUserHandler(IMessageResolverService resolver) : IServiceBusMessageHandler
    {
        public string QueueName => UserQueues.PaymentUserRequest;

        public async Task<object?> HandleAsync(ServiceBusReceivedMessage request)
        {
            var input = request.Body.ToObjectFromJson<UserRequestModel>();
            return await resolver.GetByIdOrEmailAddressAsync(input!.ApiKey, input.IdOrEmailAddress!, input.IncludePassword);
        }
    }
}
