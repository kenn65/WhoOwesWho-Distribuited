using System.ComponentModel.DataAnnotations;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EncryptionService.Services.ServiceBus.Resolvers
{
    public interface IMessageResolverService
    {
        Task<ProtectionResponseModel> ProtectAsync([Required] string apiKey, [Required] string text);
        Task<ProtectionResponseModel> UnprotectAsync([Required] string apiKey, [Required] string text);
        Task<EncryptedCookiesResponseModel> ProtectCookiesAsync([Required] CookiesRequestModel cookies);
    }

    public class MessageResolverService(ISecurityService securityService, IEncryptionService encryptionService) : IMessageResolverService
    {
        public async Task<ProtectionResponseModel> ProtectAsync([Required] string apiKey, [Required] string text)
        {
            if (!await securityService.ValidateApiKey(apiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key");
            }
            return await encryptionService.Encrypt(text);
        }

        public async Task<ProtectionResponseModel> UnprotectAsync([Required] string apiKey, [Required] string text)
        {

            if (!await securityService.ValidateApiKey(apiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key");
            }
            return await encryptionService.Decrypt(text);

        }

        public async Task<EncryptedCookiesResponseModel> ProtectCookiesAsync(CookiesRequestModel request)
        {

            if (!await securityService.ValidateApiKey(request.ApiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key");
            }
            return await encryptionService.EncryptCookies(request);
        }
    }
}
