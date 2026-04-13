using Microsoft.Extensions.Configuration;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Protection;
using WhoOwesWho.WebApp.Infrastructure.Base;
using WhoOwesWho.WebApp.UseCases.Protection.PluginInterfaces;

namespace WhoOwesWho.WebApp.Infrastructure.Protection
{
    public class ProtectionPlugin(IConfiguration configuration) : ApiPluginClientBase(configuration), IProtection
    {
        private readonly IConfiguration configuration = configuration;

        public async Task<ProtectionResponseModel> ProtectAsync(string text)
        {
            var baseAddress = configuration["EncryptionMicroService:BaseAddress"];
            var apiKey = configuration["EncryptionMicroService:Security:ApiKey"];
            var endpoint = $"{baseAddress}/protect/{text}";
            return await GetAsync<ProtectionResponseModel>(endpoint, apiKey!, true);
        }

        public async Task<ProtectionResponseModel> UnprotectAsync(string text)
        {
            var baseAddress = configuration["EncryptionMicroService:BaseAddress"];
            var apiKey = configuration["EncryptionMicroService:Security:ApiKey"];
            var endpoint = $"{baseAddress}/unprotect/{text}";
            return await GetAsync<ProtectionResponseModel>(endpoint, apiKey!, true);
        }

        public async Task<ProtectionResponseModel> ProtectCookiesAsync(CookiesRequestModel request)
        {
            var baseAddress = configuration["EncryptionMicroService:BaseAddress"];
            var apiKey = configuration["EncryptionMicroService:Security:ApiKey"];
            var endpoint = $"{baseAddress}/cookies/protect";
            return await PostAsync<ProtectionResponseModel, CookiesRequestModel>(endpoint, request, apiKey!, true);
        }
    }
}
