using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models.Base.ServiceBus;

namespace WhoOwesWho.EventService.Services.ServiceBus.Receivers
{
    public class EventReceiver : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEnumerable<IQueueHandlerRegistration> _registrations;
        private readonly List<ServiceBusProcessor> _processors = new();


        public EventReceiver(ServiceBusClient client, IServiceScopeFactory scopeFactory, IEnumerable<IQueueHandlerRegistration> registrations)
        {
            _client = client;
            _scopeFactory = scopeFactory;
            _registrations = registrations;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var reg in _registrations)
            {
                var processor = _client.CreateProcessor(reg.QueueName, new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1
                });


                processor.ProcessMessageAsync += async args =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var handler = (IServiceBusMessageHandler)scope.ServiceProvider.GetRequiredService(reg.HandlerType);


                    try
                    {
                        var result = await handler.HandleAsync(args.Message);


                        if (!string.IsNullOrWhiteSpace(args.Message.ReplyTo) && result != null)
                        {
                            var sender = _client.CreateSender(args.Message.ReplyTo);
                            var response = new ServiceBusMessage(BinaryData.FromObjectAsJson(result))
                            {
                                CorrelationId = args.Message.CorrelationId
                            };


                            await sender.SendMessageAsync(response);
                        }


                        await args.CompleteMessageAsync(args.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Handler error for queue '{reg.QueueName}': {ex}");
                        // optionally dead-letter
                        await args.DeadLetterMessageAsync(args.Message, ex.Message);
                    }
                };


                processor.ProcessErrorAsync += args =>
                {
                    Console.WriteLine($"Processor error for queue '{reg.QueueName}': {args.Exception}");
                    return Task.CompletedTask;
                };


                await processor.StartProcessingAsync(stoppingToken);
                _processors.Add(processor);


                Console.WriteLine($"Started processor for queue: {reg.QueueName}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var p in _processors)
            {
                await p.StopProcessingAsync(cancellationToken);
                await p.DisposeAsync();
            }
        }
    }
}
