namespace WhoOwesWho.AuthorizationService.Settings
{
    public class AppSettings(IConfiguration configuration)
    {
        public string ApiKeyHeaderName => configuration["Security:ApiKeyHeaderName"]!;
        public string ApiKey => configuration["Security:ApiKey"]!;
        public string AuthorizationJwtSecret => configuration["Authorization:JwtSecret"]!;
        public string AuthorizationIssuer => configuration["Authorization:Issuer"]!;
        public string AuthorizationAudience => configuration["Authorization:Audience"]!;
        public string EncryptionMicroServiceBaseAddress => configuration["EncryptionMicroService:BaseAddress"]!;
        public string EncryptionMicroServiceApiKey => configuration["EncryptionMicroService:Security:ApiKey"]!;
        public string MessagingMicroServiceBaseAddress => configuration["MessagingMicroService:BaseAddress"]!;
        public string MessagingMicroServiceApiKey => configuration["MessagingMicroService:Security:ApiKey"]!;
        public string UserMicroServiceBaseAddress => configuration["UserMicroService:BaseAddress"]!;
        public string UserMicroServiceApiKey => configuration["UserMicroService:Security:ApiKey"]!;

    }
}
