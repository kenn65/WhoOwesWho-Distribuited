namespace WhoOwesWho.Shared.Models.Base.ServiceBus
{
    public struct ServiceBusTopics
    {
        public struct MessagingTopics
        {
            public const string MessagingDispatchRequest = "whooweswho-messaging-dispatch-request";
            public const string MessagingDispatchSucceeded = "whooweswho-messaging-dispatch-succeeded";
            public const string MessagingDispatchFailed = "whooweswho-messaging-dispatch-failed";

            public const string AuthenticationUserDispatchRequest = "whooweswho-authentication-user-dispatch-request";
            public const string AuthenticationDispatchSucceeded = "whooweswho-authentication-user-dispatch-succeeded";
            public const string AuthenticationDispatchFailed = "whooweswho-authentication-user-dispatch-failed";

            public const string PaymentEventDispatchRequest = "whooweswho-payment-event-dispatch-request";
            public const string PaymentEventDispatchSucceeded = "whooweswho-payment-event-dispatch-succeeded";
            public const string PaymentEventDispatchFailed = "whooweswho-payment-event-dispatch-failed";

            public const string PaymentEventUsersDispatchRequest = "whooweswho-payment-event-users-dispatch-request";
            public const string PaymentEventUsersDispatchSucceeded = "whooweswho-payment-event-users-dispatch-succeeded";
            public const string PaymentEventUsersDispatchFailed = "whooweswho-payment-event-users-dispatch-failed";
        }
    }
}
