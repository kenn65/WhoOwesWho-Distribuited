using Azure.Messaging.ServiceBus;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services.ServiceBus.Resolvers;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.Shared.Models.Base.ServiceBus;
using static WhoOwesWho.Shared.Models.Base.ServiceBus.ObservabilityRecords;

namespace WhoOwesWho.PaymentService.Services.ServiceBus.Receivers
{
    public class EventReceiver(ServiceBusClient client, IServiceScopeFactory scopeFactory)
    {
        private ServiceBusProcessor? _requestProcessor;
        private ServiceBusSender? _succeededSender;
        private ServiceBusSender? _failedSender;

        public async Task StartAsync(CancellationToken ct)
        {
            // Command consumer
            _requestProcessor = client.CreateProcessor(
                ServiceBusTopics.MessagingTopics.PaymentEventDispatchRequest,
                "payment",
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1
                });

            // Observability publishers
            _succeededSender = client.CreateSender(
                ServiceBusTopics.MessagingTopics.PaymentEventDispatchSucceeded);

            _failedSender = client.CreateSender(
                ServiceBusTopics.MessagingTopics.PaymentEventDispatchFailed);

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
            var resolver = scope.ServiceProvider.GetRequiredService<IEventResolverService>();

            var request = args.Message.Body
                .ToObjectFromJson<EventMessageRequestModel>();

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
                await resolver.CreateEventAsync(request);

                // ---- publish succeeded event ----
                var succeeded = new UserDispatchEvent(
                    UserMessageRequestModel.Type,
                    request.Id,
                    request.Name,
                    DateTimeOffset.Now
                    );

                await _succeededSender!.SendMessageAsync(
                    new ServiceBusMessage(BinaryData.FromObjectAsJson(succeeded)));

                await args.CompleteMessageAsync(args.Message);

                Console.WriteLine(
                    $"[MESSAGING][SUCCESS] Type={UserMessageRequestModel.Type} EmailAddress={request?.Name}");
            }
            catch (Exception ex)
            {
                var failed = new UserDispatchFailedEvent(
                    UserMessageRequestModel.Type,
                    request.Id,
                    request.Name,
                    ex.Message,
                    DateTimeOffset.Now);

                await _failedSender!.SendMessageAsync(
                    new ServiceBusMessage(BinaryData.FromObjectAsJson(failed)));

                Console.WriteLine(
                    $"[MESSAGING][FAILED] Type={UserMessageRequestModel.Type} User={request.Name} Reason={ex.Message}");
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
