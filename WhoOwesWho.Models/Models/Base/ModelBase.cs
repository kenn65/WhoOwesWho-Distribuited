using Newtonsoft.Json;

namespace WhoOwesWho.Models.Models.Base
{
    public struct ServiceBusTopics
    {
        public struct MessagingTopics
        {
            public const string MessagingDispatchRequest = "whooweswho-messaging-dispatch-request";
            public const string MessagingDispatchSucceeded = "whooweswho-messaging-dispatch-succeeded";
            public const string MessagingDispatchFailed = "whooweswho-messaging-dispatch-failed";
        }
    }
    

    public abstract class ModelBase
    {
        [JsonProperty("success")]
        public bool Success { get; set; } = false;

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("exceptionMessage")]
        public string? ExceptionMessage { get; set; }
    }
}

