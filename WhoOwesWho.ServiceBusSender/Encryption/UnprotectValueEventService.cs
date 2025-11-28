using Azure.Messaging.ServiceBus;
using System.Text.Json;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Base;
using WhoOwesWho.Models.Models.Base.ServiceBus;

namespace WhoOwesWho.ServiceBusSender.Encryption
{
    public interface IUnprotectValueEventService
    {
        Task ProcessRequestAsync(string value);
        Task<T> GetResult<T>();
    }

    public class UnprotectValueEventService : EventServiceSenderBase, IUnprotectValueEventService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _responseProcessor;

        public UnprotectValueEventService(ServiceBusClient client,  ) : base(client)
        {
            _client = client;
            _responseProcessor = _client.CreateProcessor(AuthorizationEncryptionQueues.AuthorizationEncryptionResponse.ToString(), new ServiceBusProcessorOptions());
            _responseProcessor.ProcessMessageAsync += ProcessResponseAsync;
            _responseProcessor.ProcessErrorAsync += async args => Console.WriteLine(args.Exception.ToString());
            _responseProcessor.StartProcessingAsync();
        }

        public async Task ProcessRequestAsync(string value)
        {
            await Send(value, AuthorizationEncryptionQueues.AuthorizationEncryptionUnprotectRequest.ToString(), AuthorizationEncryptionQueues.AuthorizationEncryptionResponse.ToString());
        }

        protected override async Task ProcessResponseAsync(ProcessMessageEventArgs args)
        {
            var response = args.Message.Body.ToString();
            var result = JsonSerializer.Deserialize<ProtectionResponseModel>(response);
            if (!string.IsNullOrEmpty(result?.UnprotectedValue))
            {
                await SetResult(result.UnprotectedValue);
            }
            await args.CompleteMessageAsync(args.Message);
            Console.WriteLine($"Received response: {response}");
        }
    }
}
