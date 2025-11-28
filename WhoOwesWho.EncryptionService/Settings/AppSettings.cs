namespace WhoOwesWho.EncryptionService.Settings
{
    public class AppSettings(IConfiguration configuration)
    {
        public string ApiKeyHeaderName => configuration["Security:ApiKeyHeaderName"]!;
        public string ApiKey => configuration["Security:ApiKey"]!;
        public string EncryptionKey => configuration["Encryption:Key"]!;
        
    }
}
