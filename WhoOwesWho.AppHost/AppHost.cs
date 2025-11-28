

var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder
    .AddAzureServiceBus("sbemulatorns")
    .RunAsEmulator(c => c
        .WithLifetime(ContainerLifetime.Persistent));

serviceBus.AddServiceBusQueue("AuthorizationUnprotectRequest");
serviceBus.AddServiceBusQueue("AuthorizationProtectCookiesRequest");
serviceBus.AddServiceBusQueue("MessagingProtectRequest");
serviceBus.AddServiceBusQueue("EventProtectRequest");
serviceBus.AddServiceBusQueue("EventUnprotectRequest");
serviceBus.AddServiceBusQueue("PaymentProtectReuest");
serviceBus.AddServiceBusQueue("PaymentUnprotectRequest");
serviceBus.AddServiceBusQueue("UserProtectRequest");
serviceBus.AddServiceBusQueue("UserUnprotectRequest");
serviceBus.AddServiceBusQueue("AuthorizatonUnprotectResponse");
serviceBus.AddServiceBusQueue("AuthorizationProtectCookiesResponse");
serviceBus.AddServiceBusQueue("MessagingProtectResponse");
serviceBus.AddServiceBusQueue("EventProtectResponse");
serviceBus.AddServiceBusQueue("EventUnprotectResponse");
serviceBus.AddServiceBusQueue("PaymentProtectResponse");
serviceBus.AddServiceBusQueue("PaymentUnprotectResponse");
serviceBus.AddServiceBusQueue("UserProtectResponse");
serviceBus.AddServiceBusQueue("UserUnprotectResponse");

serviceBus.AddServiceBusQueue("AuthorizationUserRequest");
serviceBus.AddServiceBusQueue("EventUserRequest");
serviceBus.AddServiceBusQueue("PaymentUserRequest");
serviceBus.AddServiceBusQueue("AuthorizationUserResponse");
serviceBus.AddServiceBusQueue("EventUserResponse");
serviceBus.AddServiceBusQueue("PaymentUserResponse");

serviceBus.AddServiceBusQueue("SignUpRequest");
serviceBus.AddServiceBusQueue("AuthenticationValidateRequest");
serviceBus.AddServiceBusQueue("ForgotPasswordRequest");
serviceBus.AddServiceBusQueue("SignUpResponse");
serviceBus.AddServiceBusQueue("AuthenticationValidateResponse");
serviceBus.AddServiceBusQueue("ForgotPasswordResponse");

serviceBus.AddServiceBusQueue("CurrencyRequest");
serviceBus.AddServiceBusQueue("CurrenciesRequest");
serviceBus.AddServiceBusQueue("ExchangeRateRequest");
serviceBus.AddServiceBusQueue("CurrencyResponse");
serviceBus.AddServiceBusQueue("CurrenciesResponse");
serviceBus.AddServiceBusQueue("ExchangeRateResponse");

serviceBus.AddServiceBusQueue("UserEventRequest");
serviceBus.AddServiceBusQueue("PaymentEventRequest");
serviceBus.AddServiceBusQueue("EventUsersRequest");
serviceBus.AddServiceBusQueue("UserEventUsersRequest");
serviceBus.AddServiceBusQueue("PaymentEventUsersRequest");
serviceBus.AddServiceBusQueue("PaymentUserEventRequest");
serviceBus.AddServiceBusQueue("UserEventResponse");
serviceBus.AddServiceBusQueue("PaymentEventResponse");
serviceBus.AddServiceBusQueue("EventUsersResponse");
serviceBus.AddServiceBusQueue("UserEventUsersResponse");
serviceBus.AddServiceBusQueue("PaymentEventUsersResponse");
serviceBus.AddServiceBusQueue("PaymentUserEventResponse");

var authorizationService = builder.AddProject<Projects.WhoOwesWho_AuthorizationService>("authorizationservice")
     .WithReference(serviceBus)
     .WaitFor(serviceBus);

var currencyService = builder.AddProject<Projects.WhoOwesWho_CurrencyService>("currencyservice")
     .WithReference(serviceBus)
     .WaitFor(serviceBus);

var encryptionService =builder.AddProject<Projects.WhoOwesWho_EncryptionService>("encryptionservice")
     .WithReference(serviceBus)
     .WaitFor(serviceBus);


var eventService = builder.AddProject<Projects.WhoOwesWho_EventService>("eventservice")
     .WithReference(serviceBus)
     .WaitFor(serviceBus);


var messagingService = builder.AddProject<Projects.WhoOwesWho_MessagingService>("messagingservice")
     .WithReference(serviceBus)
     .WaitFor(serviceBus);


var paymentService = builder.AddProject<Projects.WhoOwesWho_PaymentService>("paymentservice")
     .WithReference(serviceBus)
     .WaitFor(serviceBus);


var userService = builder.AddProject<Projects.WhoOwesWho_UserService>("userservice")
     .WithReference(serviceBus)
     .WaitFor(serviceBus);


authorizationService.WithReference(serviceBus);
currencyService.WithReference(serviceBus);
encryptionService.WithReference(serviceBus);
eventService.WithReference(serviceBus);
messagingService.WithReference(serviceBus);
paymentService.WithReference(serviceBus);
userService.WithReference(serviceBus);

builder.Build().Run();
