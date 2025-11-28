namespace WhoOwesWho.MessagingService.Settings
{
    public class AppSettings(IConfiguration configuration)
    {
        public string? ApiKeyHeaderName => configuration["Security:ApiKeyHeaderName"];
        public string? ApiKey => configuration["Security:ApiKey"];
        public string? EncryptionMicroServiceBaseAddress => configuration["EncryptionMicroService:BaseAddress"];
        public string? EncryptionMicroServiceApiKey => configuration["EncryptionMicroService:Security:ApiKey"];
        public string? SignUpTemplatePath => configuration["Templates:SignUp:Path"];
        public string? SignUpTemplateSubject => configuration["Templates:SignUp:Subject"];
        public string? ResetPasswordTemplatePath => configuration["Templates:ResetPassword:Path"];
        public string? ResetPasswordTemplateSubject => configuration["Templates:ResetPassword:Subject"];
        public string? AuthenticationTemplatePath => configuration["Templates:Authentication:Path"];
        public string? AuthenticationTemplateSubject => configuration["Templates:Authentication:Subject"];
        public string? SmtpServer => configuration["EmailMessaging:SmtpServer"];
        public int SmtpPort => int.TryParse(configuration["EmailMessaging:SmtpPort"], out var port) ? port : 587;
        public string? SmtpUserName => configuration["EmailMessaging:SmtpUserName"];
        public string? SmtpPassword => configuration["EmailMessaging:SmtpPassword"];
    }
}
