using System.ComponentModel.DataAnnotations;
using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Encryption;
using WhoOwesWho.UserService.Services.ServiceBus.Senders;

namespace WhoOwesWho.UserService.Services.ServiceBus.Resolvers
{       
    public interface IMessageResolverService
    {
        Task<UserModel?> GetByIdOrEmailAddressAsync([Required] string apiKey, [Required] string idOrEmailAddress, [Required] bool complete);
    }
    public class MessageResolverService(
        IDataQueryService dataSelectionService, 
        IValidationService validationService, 
        IUnprotectValueMessageSender unprotectValueMessageSender, ISecurityService securityService) 
        : IMessageResolverService
    {
        public async Task<UserModel?> GetByIdOrEmailAddressAsync([Required] string apiKey, [Required] string idOrEmailAddress, [Required] bool complete)
        {
            if (!await securityService.ValidateApiKey(apiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key");
            }
            var unprotectedValue = await unprotectValueMessageSender.SendAsync(idOrEmailAddress);
            var checkEmail = (await validationService.ValidateEmailAsync(unprotectedValue, true));

            return await Task.FromResult(checkEmail.isValid
                    ? await dataSelectionService.GetSingleUserByEmailAddressAsync(unprotectedValue, complete)
                    : await dataSelectionService.GetSingleUserByIdAsync(Guid.Parse(unprotectedValue), complete));

            
        }
    }
}
