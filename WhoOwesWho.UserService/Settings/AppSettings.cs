namespace WhoOwesWho.UserService.Settings
{
    public class AppSettings(IConfiguration configuration)
    {
        public string ApiKeyHeaderName => configuration["Security:ApiKeyHeaderName"]!;
        public string ApiKey => configuration["Security:ApiKey"]!;
        public string DatabaseConnectionString => configuration["Database:ConnectionString"]!;
        public string PasswordLengthRequired => configuration["Password:Format:LenghtRequired"]!;
        public string PasswordUppercaseRequired => configuration["Password:Format:UppercaseRequired"]!;
        public string PasswordDigitsRequired => configuration["Password:Format:DigitsRequired"]!;
        public string ForgotPasswordExpirationTimeInMinutes => configuration["Password:ForgotPassword:ExpirationTimeInMinutes"]!;
        public string ForgotPasswordTokenSecret => configuration["Password:ForgotPassword:TokenSecret"]!;
        public string EncryptionMicroServiceBaseAddress => configuration["EncryptionMicroService:BaseAddress"]!;
        public string EncryptionMicroServiceApiKey => configuration["EncryptionMicroService:Security:ApiKey"]!;
        public string MessagingMicroServiceBaseAddress => configuration["MessagingMicroService:BaseAddress"]!;
        public string MessagingMicroServiceApiKey => configuration["MessagingMicroService:Security:ApiKey"]!;
        public string AuthorizationMicroServiceApiKey => configuration["AuthorizationMicroService:Security:ApiKey"]!;
        public string? EventMicroServiceEventsBaseAddress => configuration["EventMicroService:EventsBaseAddress"]!;
        public string? EventMicroServiceEventUsersBaseAddress = configuration["EventMicroService:EventUsersBaseAddress"]!;
        public string? EventMicroServiceUserEventsBaseAddress = configuration["EventMicroService:UserEventsBaseAddress"]!;
        public string? EventMicroServiceApiKey => configuration["EventMicroService:Security:ApiKey"]!;
    }
}
