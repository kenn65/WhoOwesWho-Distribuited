using Microsoft.AspNetCore.Components;
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
    IProtectionUseCase protectionUseCase,
    IAlertService alertService,
    NavigationManager nav)
{
    private EventUserAssignmentResponseModel? activeUserAssignment;
    private PaymentsResponseModel? payments;
    private CookiesResponseModel? cookies;
    private string eventId = string.Empty;
    private Guid userId = Guid.Empty;
    private bool isAdministrator;
    private bool isLoading = true;
    private bool isProcessing = false;

    [SupplyParameterFromForm]
    private SettleEventRequestModel? settleEventRequestModel { get; set; }

    protected override async Task OnInitializedAsync()
    {
        cookies = await cookiesMasterService.GetAsync();
        isAdministrator = await cookiesMasterService.IsAdministratorAsync(cookies!);
        userId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(cookies!.UserIdValue));
        activeUserAssignment = await GetActiveUserAssignmentAsync();
        eventId = activeUserAssignment.EventId.ToString();
        payments = await GetPaymentsAsync();
        settleEventRequestModel ??= new();
        isLoading = false;
    }

    private async Task<EventUserAssignmentResponseModel> GetActiveUserAssignmentAsync()
    {
        return await eventsUseCase.ExecuteGetUserAssignmentAsync(userId, true, cookies!.TokenValue);
    }

    private async Task<PaymentsResponseModel> GetPaymentsAsync()
    {
        return await paymentsUseCase.ExecuteAsync(Guid.Parse(eventId), true, cookies!.TokenValue);
    }

    private async Task SettleEventAsync()
    {
        var confirmation = await alertService.Confirm("Are you sure you want to settle and close this event?");
        if (confirmation)
        {
            isProcessing = true;
            var requestModel = new SettleEventRequestModel
            {
                EventIdString = eventId
            };
            var response = await eventsUseCase.ExecuteSettleEventAsync(requestModel, cookies!.TokenValue);
            await StopProcessingAsync();
            if (!response.Success)
            {
                await alertService.Error(response.Message!);
                nav.NavigateTo("/me/payments", true);
                return;
            }
            await alertService.Success(response.Message!);
            nav.NavigateTo("/me/settlements");
        }
    }

    private async Task StopProcessingAsync()
    {
        isProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}
