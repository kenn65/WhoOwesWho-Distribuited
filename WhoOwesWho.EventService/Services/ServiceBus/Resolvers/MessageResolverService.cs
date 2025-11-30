using System.ComponentModel.DataAnnotations;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EventService.Services.ServiceBus.Handling
{
    public interface IMessageResolverService
    {
        Task<EventResponseModel> GetEventAsync([Required] string apiKey, [Required] string eventId, [Required] bool active);
        Task<EventResponseModel?> GetEventByUserAsync([Required] string apiKey, [Required] string userId, [Required] bool active);
        Task<IEnumerable<UserModel?>> GetEventUsersAsync([Required] string apiKey, [Required] string eventId, [Required] bool active);
    }

    public class MessageResolverService(ISecurityService securityService, IDataQueryService dataSelectionService) : IMessageResolverService
    {
        public async Task<EventResponseModel> GetEventAsync([Required] string apiKey, [Required] string eventId, [Required] bool active)
        {
            if (!await securityService.ValidateApiKey(apiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key");
            }
            var result = await dataSelectionService.GetEventAsync(Guid.Parse(eventId), active);
            return await Task.FromResult(result!);
            
        }

        public async Task<EventResponseModel?> GetEventByUserAsync([Required] string apiKey, [Required] string userId, [Required] bool active)
        {
            if (!await securityService.ValidateApiKey(apiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key");
            }
            var result = await dataSelectionService.GetEventByUserAsync(userId, active);
            return await Task.FromResult(result!);
        }

        public async Task<IEnumerable<UserModel?>> GetEventUsersAsync([Required] string apiKey, [Required] string eventId, [Required] bool active)
        {
            if (!await securityService.ValidateApiKey(apiKey))
            {
                throw new UnauthorizedAccessException("Invalid API Key");
            }
            return await dataSelectionService.GetEventUsersAsync(eventId, active);
        }
    }
}
