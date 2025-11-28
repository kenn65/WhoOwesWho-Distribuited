namespace WhoOwesWho.EventService.Settings
{
    public class AppSettings(IConfiguration configuration)
    {
        public string? ApiKeyHeaderName => configuration["Security:ApiKeyHeaderName"];
        public string? ApiKey => configuration["Security:ApiKey"];
        public string? DatabaseConnectionString => configuration["Database:ConnectionString"];
        public string? EncryptionMicroServiceBaseAddress => configuration["EncryptionMicroService:BaseAddress"];
        public string? EncryptionMicroServiceApiKey => configuration["EncryptionMicroService:Security:ApiKey"];
        public string? MessagingMicroServiceBaseAddress => configuration["MessagingMicroService:BaseAddress"];
        public string? MessagingMicroServiceApiKey => configuration["MessagingMicroService:Security:ApiKey"];
        public string? UserMicroServiceBaseAddress => configuration["UserMicroService:BaseAddress"];
        public string? UserMicroServiceApiKey => configuration["UserMicroService:Security:ApiKey"];
        public string? CurrencyMicroServiceBaseAddress => configuration["CurrencyMicroService:BaseAddress"];
        public string? CurrencyMicroServiceApiKey => configuration["CurrencyMicroService:Security:ApiKey"];

    }
}
