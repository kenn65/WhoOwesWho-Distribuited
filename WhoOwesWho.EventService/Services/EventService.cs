using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.EventService.Services.Gateways;

namespace WhoOwesWho.EventService.Services
{
    public interface IEventService
    {
        Task<AssignmentResponseModel> AssignAsync(AssignmentRequestModel request);
    }

    public class EventService (
        IConfiguration configuration, 
        IDataMutationService dataModificationService, 
        IDataQueryService dataSelectionService,
        IUserGatewayService userGatewayService
        ) : ServiceBase(configuration), IEventService
    {
        public async Task<AssignmentResponseModel> AssignAsync(AssignmentRequestModel request)
        {
            var you = await userGatewayService.GetAuthorizedUserAsync(request.UserId!, request.Token!, true, false);

            var users = (await dataSelectionService.GetEventUsersAsync(request.EventId!, request.Token!)).ToList();
            if (users.Any(u => u.Admin) && you.Admin)
            {
                return await Task.FromResult(new AssignmentResponseModel
                {
                    Message =
                        "You cannot assign to this event as an administrator, because an event administrator already exists."
                });
            }

            var response = await dataModificationService.AssignToEventAsync(request);
            return await Task.FromResult(response);
        }
    }
}
