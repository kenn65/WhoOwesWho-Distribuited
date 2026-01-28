using Azure.Messaging.ServiceBus;
using WhoOwesWho.Models.Models.Base;
using static WhoOwesWho.Models.Models.Base.ServiceBus.ObservabilityRecords;

public sealed class MessagingReceiver
{
    private readonly ServiceBusClient _client;
    private ServiceBusProcessor? _success;
    private ServiceBusProcessor? _failed;

    public MessagingReceiver(ServiceBusClient client)
    {
        _client = client;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _success = _client.CreateProcessor(
            ServiceBusTopics.MessagingTopics.MessagingDispatchSucceeded,
            "messaging-observability");

        _failed = _client.CreateProcessor(
            ServiceBusTopics.MessagingTopics.MessagingDispatchFailed,
            "messaging-observability");

        _success.ProcessMessageAsync += HandleSuccessAsync;
        _success.ProcessErrorAsync += HandleErrorAsync;

        _failed.ProcessMessageAsync += HandleFailedAsync;
        _failed.ProcessErrorAsync += HandleErrorAsync;

        await _success.StartProcessingAsync(ct);
        await _failed.StartProcessingAsync(ct);
    }

    public async Task StopAsync(CancellationToken ct)
    {
        if (_success != null)
        {
            await _success.StopProcessingAsync(ct);
            await _success.DisposeAsync();
        }

        if (_failed != null)
        {
            await _failed.StopProcessingAsync(ct);
            await _failed.DisposeAsync();
        }
    }

    // ----------------------------
    // MESSAGE HANDLERS
    // ----------------------------

    private async Task HandleSuccessAsync(ProcessMessageEventArgs args)
    {
        var evt = args.Message.Body
            .ToObjectFromJson<MessagingDispatchedEvent>();

        Console.WriteLine(
            $"[MESSAGING][SUCCESS] Type={evt?.Type} User={evt?.UserEmail}");

        await args.CompleteMessageAsync(args.Message);
    }

    private async Task HandleFailedAsync(ProcessMessageEventArgs args)
    {
        var evt = args.Message.Body
            .ToObjectFromJson<MessagingDispatchFailedEvent>();

        Console.WriteLine(
            $"[MESSAGING][FAILED] Type={evt?.Type} User={evt?.UserEmail} Reason={evt?.Reason}");

        await args.CompleteMessageAsync(args.Message);
    }

    // ----------------------------
    // ERROR HANDLER
    // ----------------------------

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine(
            $"[MESSAGING][ERROR] " +
            $"Entity={args.EntityPath} " +
            $"ErrorSource={args.ErrorSource} " +
            $"Exception={args.Exception}");

        return Task.CompletedTask;
    }
}
