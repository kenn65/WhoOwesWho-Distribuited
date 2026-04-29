using Microsoft.Extensions.Configuration;

namespace WhoOwesWho.WebApp.Infrastructure.Settings
{
    public class AppSettings(IConfiguration configuration)
    {
        public string? ApiKeyHeaderName => configuration["Security:ApiKeyHeaderName"];
        public string? AuthorizationMicroserviceApiKey => configuration["AuthorizationMicroservice:Security:ApiKey"];
        public string? AuthorizationMicroserviceBaseAddress => configuration["AuthorizationMicroservice:BaseAddress"];
        public string? EncryptionMicroserviceApiKey => configuration["EncryptionMicroservice:Security:ApiKey"];
        public string? EncryptionMicroserviceBaseAddress => configuration["EncryptionMicroservice:BaseAddress"];
        public string? MessagingMicroserviceApiKey => configuration["MessagingMicroservice:Security:ApiKey"];
        public string? MessagingMicroserviceBaseAddress => configuration["MessagingMicroservice:BaseAddress"];
        public string? UserMicroserviceApiKey => configuration["UserMicroservice:Security:ApiKey"];
        public string? UserMicroserviceBaseAddress => configuration["UserMicroservice:BaseAddress"];
        public string? CurrencyMicroserviceBaseAddress => configuration["CurrencyMicroservice:BaseAddress"];
        public string? CurrencyMicroserviceApiKey => configuration["CurrencyMicroservice:Security:ApiKey"];
        public string? EventMicroserviceEventsBaseAddress => configuration["EventMicroservice:EventsBaseAddress"]!;
        public string? EventMicroserviceEventUsersBaseAddress = configuration["Eventmicroservice:EventUsersBaseAddress"]!;
        public string? EventMicroserviceUserEventsBaseAddress = configuration["EventMicroservice:UserEventsBaseAddress"]!;
        public string? EventMicroserviceApiKey => configuration["EventMicroservice:Security:ApiKey"]!;
        public string? PaymentMicroserviceBaseAddress => configuration["PaymentMicroservice:PaymentsBaseAddress"]!;
        public string? PaymentMicroserviceBalanceBaseAddress => configuration["PaymentMicroservice:BalaceBaseAddress"]!;
        public string? PaymentMicroserviceSettlementsBaseAddress => configuration["PaymentMicroservice:SettlementsBaseAddress"];
        public string PaymentMicroserviceApiKey => configuration["PaymentMicroservice:Security:ApiKey"]!;
    }
}

