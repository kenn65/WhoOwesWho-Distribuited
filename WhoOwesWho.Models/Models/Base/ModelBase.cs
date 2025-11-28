using Newtonsoft.Json;

namespace WhoOwesWho.Models.Models.Base
{
    public struct Queues
    {
        public struct EncryptionQueues
        {
            public const string AuthorizationUnprotectRequest = "AuthorizationUnprotectRequest";
            public const string AuthorizationProtectCookiesRequest = "AuthorizationProtectCookiesRequest";
            public const string MessagingProtectRequest = "MessagingProtectRequest";
            public const string EventProtectRequest = "EventProtectRequest";
            public const string EventUnprotectRequest = "EventUnprotectRequest";
            public const string PaymentProtectReuest = "PaymentProtectReuest";
            public const string PaymentUnprotectRequest = "PaymentUnprotectRequest";
            public const string UserProtectRequest = "UserProtectRequest";
            public const string UserUnprotectRequest = "UserUnprotectRequest";

            public const string AuthorizatonUnprotectResponse = "AuthorizatonUnprotectResponse";
            public const string AuthorizationProtectCookiesResponse = "AuthorizationProtectCookiesResponse";
            public const string MessagingProtectResponse = "MessagingProtectResponse";
            public const string EventProtectResponse = "EventProtectResponse";
            public const string EventUnprotectResponse = "EventUnprotectResponse";
            public const string PaymentProtectResponse = "PaymentProtectResponse";
            public const string PaymentUnprotectResponse = "PaymentUnprotectResponse";
            public const string UserProtectResponse = "UserProtectResponse";
            public const string UserUnprotectResponse = "UserUnprotectResponse";
        }

        public struct UserQueues
        {
            public const string AuthorizationUserRequest = "AuthorizationUserRequest";
            public const string EventUserRequest = "EventUserRequest";
            public const string PaymentUserRequest = "PaymentUserRequest";

            public const string AuthorizationUserResponse = "AuthorizationUserResponse";
            public const string EventUserResponse = "EventUserResponse";
            public const string PaymentUserResponse = "PaymentUserResponse";
        }

        public struct MessagingQueues
        {
            public const string SignUpRequest = "SignUpRequest";
            public const string AuthenticationValidateRequest = "AuthenticationValidateRequest";
            public const string ForgotPasswordRequest = "ForgotPasswordRequest";

            public const string SignUpResponse = "SignUpResponse";
            public const string AuthenticationValidateResponse = "AuthenticationValidateResponse";
            public const string ForgotPasswordResponse = "ForgotPasswordResponse";
        }

        public struct CurrencyQueues
        {
            public const string CurrencyRequest = "CurrencyRequest";
            public const string CurrenciesRequest = "CurrenciesRequest";
            public const string ExchangeRateRequest = "ExchangeRateRequest";

            public const string CurrencyResponse = "CurrencyResponse";
            public const string CurrenciesResponse = "CurrenciesResponse";
            public const string ExchangeRateResponse = "ExchangeRateResponse";

        }

        public struct EventQueues
        {
            public const string UserEventRequest = "UserEventRequest";
            public const string PaymentEventRequest = "PaymentEventRequest";
            public const string EventUsersRequest = "EventUsersRequest";
            public const string UserEventUsersRequest = "UserEventUsersRequest";
            public const string PaymentEventUsersRequest = "PaymentEventUsersRequest";
            public const string PaymentUserEventRequest = "PaymentUserEventRequest";


            public const string UserEventResponse = "UserEventResponse";
            public const string PaymentEventResponse = "PaymentEventResponse";
            public const string EventUsersResponse = "EventUsersResponse";
            public const string UserEventUsersResponse = "UserEventUsersResponse";
            public const string PaymentEventUsersResponse = "PaymentEventUsersResponse";
            public const string PaymentUserEventResponse = "PaymentUserEventResponse";
        }
    }
    

    public abstract class ModelBase
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("exceptionMessage")]
        public string? ExceptionMessage { get; set; }
    }
}

