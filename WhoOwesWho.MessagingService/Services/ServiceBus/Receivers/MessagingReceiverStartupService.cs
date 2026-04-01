using Azure.Messaging.ServiceBus;
using WhoOwesWho.Shared.Models.Base;

namespace WhoOwesWho.MessagingService.Services.ServiceBus.Receivers
{
    public sealed class MessagingReceiverStartupService(ServiceBusClient client,MessagingReceiver receiver) : BackgroundService
    {
      

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

            await receiver.StartAsync(stoppingToken);

            Console.WriteLine("[MESSAGING] Receivers started");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await receiver.StopAsync(cancellationToken);
        }

      private async Task WaitForSubscriptionAsync(string topic, string subscription, CancellationToken ct)
        {
            for (var i = 0; i < 20; i++)
            {
                try
                {
                    await using var receiver =
                        client.CreateReceiver(topic, subscription);

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
