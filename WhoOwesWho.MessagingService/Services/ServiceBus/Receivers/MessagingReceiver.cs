using Azure.Messaging.ServiceBus;
using WhoOwesWho.MessagingService.Services.ServiceBus.Handling;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.Shared.Models.Base;
using static WhoOwesWho.Shared.Models.Base.ServiceBus.ObservabilityRecords;

namespace WhoOwesWho.MessagingService.Services.ServiceBus.Receivers
{

    public sealed class MessagingReceiver(ServiceBusClient client, IServiceScopeFactory scopeFactory)
    {
        private ServiceBusProcessor? _requestProcessor;
        private ServiceBusSender? _succeededSender;
        private ServiceBusSender? _failedSender;

        public async Task StartAsync(CancellationToken ct)
        {
            // Command consumer
            _requestProcessor = client.CreateProcessor(
                ServiceBusTopics.MessagingTopics.MessagingDispatchRequest,
                "messaging",
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1
                });

            // Observability publishers
            _succeededSender = client.CreateSender(
                ServiceBusTopics.MessagingTopics.MessagingDispatchSucceeded);

            _failedSender = client.CreateSender(
                ServiceBusTopics.MessagingTopics.MessagingDispatchFailed);

            _requestProcessor.ProcessMessageAsync += HandleRequestAsync;
            _requestProcessor.ProcessErrorAsync += HandleErrorAsync;

            await _requestProcessor.StartProcessingAsync(ct);

            Console.WriteLine("[MESSAGING] Request receiver started");
        }

        public async Task StopAsync(CancellationToken ct)
        {
            if (_requestProcessor != null)
            {
                await _requestProcessor.StopProcessingAsync(ct);
                await _requestProcessor.DisposeAsync();
            }

            if (_succeededSender != null)
                await _succeededSender.DisposeAsync();

            if (_failedSender != null)
                await _failedSender.DisposeAsync();
        }

        private async Task HandleRequestAsync(ProcessMessageEventArgs args)
        {
            using var scope = scopeFactory.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IMessageResolverService>();

            var request = args.Message.Body
                .ToObjectFromJson<MessagingRequestModel>();

            if (request is null)
            {
                await args.DeadLetterMessageAsync(
                    args.Message,
                    "InvalidPayload",
                    "Could not deserialize MessagingRequestModel");
                return;
            }

            try
            {
                await resolver.SendEmailAsync(request);

                // ---- publish succeeded event ----
                var succeeded = new MessagingDispatchedEvent(
                    request.Type,
                    request?.User?.EmailAddress,
                    DateTimeOffset.Now
                    );

                await _succeededSender!.SendMessageAsync(
                    new ServiceBusMessage(BinaryData.FromObjectAsJson(succeeded)));

                await args.CompleteMessageAsync(args.Message);

                Console.WriteLine(
                    $"[MESSAGING][SUCCESS] Type={request?.Type} User={request?.User?.EmailAddress}");
            }
            catch (Exception ex)
            {
                var failed = new MessagingDispatchFailedEvent(
                    request.Type,
                    request?.User?.EmailAddress,
                    ex.Message,
                    DateTimeOffset.Now);

                await _failedSender!.SendMessageAsync(
                    new ServiceBusMessage(BinaryData.FromObjectAsJson(failed)));

                Console.WriteLine(
                    $"[MESSAGING][FAILED] Type={request?.Type} User={request?.User?.EmailAddress} Reason={ex.Message}");
                throw;
            }
        }

        private Task HandleErrorAsync(ProcessErrorEventArgs args)
        {
            Console.WriteLine(
                $"[MESSAGING][ERROR] " +
                $"Entity={args.EntityPath} " +
                $"Source={args.ErrorSource} " +
                $"Exception={args.Exception}");

            return Task.CompletedTask;
        }
    }
}
