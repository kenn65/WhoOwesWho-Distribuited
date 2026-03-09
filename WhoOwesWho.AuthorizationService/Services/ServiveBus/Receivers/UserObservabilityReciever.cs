using Azure.Messaging.ServiceBus;
using WhoOwesWho.Shared.Models.Base;
using static WhoOwesWho.Shared.Models.Base.ServiceBus.ObservabilityRecords;

namespace WhoOwesWho.AuthorizationService.Services.ServiveBus.Receivers
{
    public sealed class UserObservabilityReciever : BackgroundService
    {
        private readonly ServiceBusProcessor _successProcessor;
        private readonly ServiceBusProcessor _failedProcessor;

        public UserObservabilityReciever(ServiceBusClient client)
        {
            _successProcessor = client.CreateProcessor(
                topicName: ServiceBusTopics.MessagingTopics.AuthenticationDispatchSucceeded,
                subscriptionName: "authentication-observability");

            _failedProcessor = client.CreateProcessor(
                topicName: ServiceBusTopics.MessagingTopics.AuthenticationDispatchFailed,
                subscriptionName: "authentication-observability");
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
            var evt = args.Message.Body.ToObjectFromJson<UserDispatchEvent>();

            Console.WriteLine(
                $"[AUTH] Credentials dispatched OK | Type={evt?.Type} | User={evt?.Email}");

            await args.CompleteMessageAsync(args.Message);
        }

        private async Task HandleFailedAsync(ProcessMessageEventArgs args)
        {
            var evt = args.Message.Body.ToObjectFromJson<UserDispatchFailedEvent>();

            Console.WriteLine(
                $"[AUTH] Credentials dispatch FAILED | Type={evt?.Type} | User={evt?.Email} | Reason={evt?.Reason}");

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
