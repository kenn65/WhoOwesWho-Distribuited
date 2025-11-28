using Azure.Messaging.ServiceBus;
using System.Collections.Concurrent;
using System.Text.Json;

namespace WhoOwesWho.Models.Models.Base.ServiceBus
{
    public abstract class EventServiceSenderBase
    {
        private readonly ServiceBusClient _client;

        // correlationId → TaskCompletionSource for the response
        private readonly ConcurrentDictionary<string, TaskCompletionSource<object>> _pending =
            new();

        protected EventServiceSenderBase(ServiceBusClient client)
        {
            _client = client;
        }

        protected async Task<Task<TResponse>> SendRequestAsync<TRequest, TResponse>(
            TRequest input,
            string requestQueue,
            string responseQueue)
        {
            var correlationId = Guid.NewGuid().ToString();

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[correlationId] = tcs;

            var sender = _client.CreateSender(requestQueue);

            var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(input))
            {
                CorrelationId = correlationId,
                ReplyTo = responseQueue
            };

            await sender.SendMessageAsync(message);


            // typed task returned
            return tcs.Task.ContinueWith(t =>
            {
                _pending.TryRemove(correlationId, out _);
                return (TResponse)t.Result;

            });
        }

        /// <summary>
        /// Called from the processor in the derived class
        /// </summary>
        protected async Task HandleResponseAsync<TResponse>(ProcessMessageEventArgs args)
        {
            if (args.Message.CorrelationId == null)
                return;

            if (_pending.TryGetValue(args.Message.CorrelationId, out var tcs))
            {
                var payload = args.Message.Body.ToString();
                var response = JsonSerializer.Deserialize<TResponse>(payload);
                tcs.SetResult(response!);
            }

            await args.CompleteMessageAsync(args.Message);
        }
    }
}
