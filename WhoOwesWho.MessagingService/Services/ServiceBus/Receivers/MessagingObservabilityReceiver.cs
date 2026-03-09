using Azure.Messaging.ServiceBus;
using WhoOwesWho.Shared.Models.Base;
using static WhoOwesWho.Shared.Models.Base.ServiceBus.ObservabilityRecords;


namespace WhoOwesWho.MessagingService.Services.ServiceBus.Receivers
{

    public sealed class MessagingObservabilityReceiver : BackgroundService
    {
        private readonly ServiceBusProcessor _successProcessor;
        private readonly ServiceBusProcessor _failedProcessor;

        public MessagingObservabilityReceiver(ServiceBusClient client)
        {
            _successProcessor = client.CreateProcessor(
                topicName: ServiceBusTopics.MessagingTopics.MessagingDispatchSucceeded,
                subscriptionName: "messaging-observability");

            _failedProcessor = client.CreateProcessor(
                topicName: ServiceBusTopics.MessagingTopics.MessagingDispatchFailed,
                subscriptionName: "messaging-observability");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // SUCCESS processor
            _successProcessor.ProcessMessageAsync += HandleSuccessAsync;
            _successProcessor.ProcessErrorAsync += HandleErrorAsync;

            // FAILED processor
            _failedProcessor.ProcessMessageAsync += HandleFailedAsync;
            _failedProcessor.ProcessErrorAsync += HandleErrorAsync;

            await _successProcessor.StartProcessingAsync(stoppingToken);
            await _failedProcessor.StartProcessingAsync(stoppingToken);
        }

        private async Task HandleSuccessAsync(ProcessMessageEventArgs args)
        {
            var evt = args.Message.Body.ToObjectFromJson<MessagingDispatchedEvent>();

            Console.WriteLine(
                $"[AUTH] Messaging dispatched OK | Type={evt?.Type} | User={evt?.UserEmail}");

            await args.CompleteMessageAsync(args.Message);
        }

        private async Task HandleFailedAsync(ProcessMessageEventArgs args)
        {
            var evt = args.Message.Body.ToObjectFromJson<MessagingDispatchFailedEvent>();

            Console.WriteLine(
                $"[AUTH] Messaging FAILED | Type={evt?.Type} | User={evt?.UserEmail} | Reason={evt?.Reason}");

            await args.CompleteMessageAsync(args.Message);
        }

        private Task HandleErrorAsync(ProcessErrorEventArgs args)
        {
            Console.WriteLine(
                $"[AUTH] ServiceBus error | Entity={args.EntityPath} | Error={args.Exception}");

            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _successProcessor.StopProcessingAsync(cancellationToken);
            await _failedProcessor.StopProcessingAsync(cancellationToken);

            await _successProcessor.DisposeAsync();
            await _failedProcessor.DisposeAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}