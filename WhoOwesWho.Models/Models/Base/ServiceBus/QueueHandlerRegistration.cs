namespace WhoOwesWho.Models.Models.Base.ServiceBus
{
    public interface IQueueHandlerRegistration
    {
        string QueueName { get; }
        Type HandlerType { get; }
    }

    public class QueueHandlerRegistration<THandler> : IQueueHandlerRegistration where THandler : IServiceBusMessageHandler
    {
        public string QueueName { get; }
        public Type HandlerType => typeof(THandler);
        public QueueHandlerRegistration(string queueName) => QueueName = queueName;
    }
}
