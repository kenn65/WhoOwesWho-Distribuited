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
            await WaitForSubscriptionAsync(
                ServiceBusTopics.MessagingTopics.MessagingDispatchSucceeded,
                "messaging-observability-succeeded",
                stoppingToken);

            await WaitForSubscriptionAsync(
                ServiceBusTopics.MessagingTopics.MessagingDispatchFailed,
                "messaging-observability-failed",
                stoppingToken);

            await _receiver.StartAsync(stoppingToken);

            Console.WriteLine("[MESSAGING] Receivers started");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _receiver.StopAsync(cancellationToken);
        }

      private async Task WaitForSubscriptionAsync(string topic, string subscription, CancellationToken ct)
        {
            for (var i = 0; i < 20; i++)
            {
                try
                {
                    await using var receiver =
                        _client.CreateReceiver(topic, subscription);

                    await receiver.PeekMessageAsync(cancellationToken: ct);

                    Console.WriteLine(
                        $"[MESSAGING] Subscription ready: {topic}/{subscription}");

                    return;
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }
            }

            throw new InvalidOperationException(
                $"Service Bus subscription {topic}/{subscription} did not become ready.");
        }
    }
}
