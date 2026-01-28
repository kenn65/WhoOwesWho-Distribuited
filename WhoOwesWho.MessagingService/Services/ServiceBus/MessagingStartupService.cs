using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models.Base;

namespace WhoOwesWho.MessagingService.Services.ServiceBus
{
    public sealed class MessagingStartupService : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly MessagingReceiver _receiver;

        public MessagingStartupService(
            ServiceBusClient client,
            MessagingReceiver receiver)
        {
            _client = client;
            _receiver = receiver;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Only wait for AMQP/data plane
            await WaitForDataPlaneAsync(stoppingToken);

            // Start receivers
            await _receiver.StartAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _receiver.StopAsync(cancellationToken);
        }

        private async Task WaitForDataPlaneAsync(CancellationToken ct)
        {
            for (var i = 0; i < 15; i++)
            {
                try
                {
                    await using var sender =
                        _client.CreateSender(
                            ServiceBusTopics.MessagingTopics.MessagingDispatchSucceeded);

                    return; // ready
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }
            }

            throw new InvalidOperationException(
                "Service Bus emulator data plane did not become ready.");
        }
    }
}
