using WhoOwesWho.EncryptionService.Settings;

namespace WhoOwesWho.EncryptionService.Services.Base
{
    public abstract class ServiceBase(IConfiguration configuration)
    {
        private readonly AppSettings _settings = new(configuration);

        protected AppSettings AppSettings => _settings;
    }
}
