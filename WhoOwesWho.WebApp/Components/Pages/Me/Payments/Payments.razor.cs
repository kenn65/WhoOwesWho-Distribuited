using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Payments;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Payments;
public partial class Payments(
    ICookiesMasterService cookiesMasterService, 
    IEventsUseCase eventsUseCase, 
    IPaymentsUseCase paymentsUseCase, 
    IProtectionUseCase protectionUseCase)
{
    private EventUserAssignmentResponseModel? activeUserAssignment;
    private PaymentsResponseModel? payments;
    private CookiesResponseModel? cookies;
    private string eventId = string.Empty;
    private Guid userId = Guid.Empty;

    protected override async Task OnInitializedAsync()
    {
        cookies = await cookiesMasterService.GetAsync();
        userId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(cookies!.UserIdValue));
        activeUserAssignment = await GetActiveUserAssignmentAsync();
        eventId = activeUserAssignment.EventId.ToString();
        payments = await GetPaymentsAsync();
    }

    private async Task<EventUserAssignmentResponseModel> GetActiveUserAssignmentAsync()
    {
        return await eventsUseCase.ExecuteGetUserAssignmentAsync(userId, cookies!.TokenValue);
    }

    private async Task<PaymentsResponseModel> GetPaymentsAsync()
    {
        return await paymentsUseCase.ExecuteAsync(Guid.Parse(eventId), true, cookies!.TokenValue);
    }
}
