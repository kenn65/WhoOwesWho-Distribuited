using Azure.Messaging.ServiceBus;

namespace WhoOwesWho.Models.Models.Base.ServiceBus
{
    public interface IServiceBusMessageHandler
    {
        string QueueName { get; }
        Task<object?> HandleAsync(ServiceBusReceivedMessage request);
    }
}
