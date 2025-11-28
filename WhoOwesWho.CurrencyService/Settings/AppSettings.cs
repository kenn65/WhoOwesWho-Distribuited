namespace WhoOwesWho.CurrencyService.Settings
{
    public class AppSettings(IConfiguration configuration)
    {
        public string? ApiKeyHeaderName => configuration["Security:ApiKeyHeaderName"];
        public string? ApiKey => configuration["Security:ApiKey"];
        public string? FreeCurrencyHost => configuration["FreeCurrencyApi:Host"];
        public string? FreeCurrencyApiKey => configuration["FreeCurrencyApi:ApiKey"];
    }
}
