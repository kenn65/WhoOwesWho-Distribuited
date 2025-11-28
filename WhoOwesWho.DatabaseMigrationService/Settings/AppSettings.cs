namespace WhoOwesWho.DatabaseMigrationService.Settings
{
    public class AppSettings (IConfiguration configuration)
    {
        public string ApiKeyHeaderName => configuration["Security:ApiKeyHeaderName"]!;
        public string ApiKey => configuration["Security:ApiKey"]!;
        public string DatabaseConnectionString => configuration["Database:ConnectionString"]!;
    }
}
