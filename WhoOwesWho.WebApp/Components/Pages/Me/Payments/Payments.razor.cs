using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Components.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Payments;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Payments;

public partial class Payments(
    IEventsUseCase eventsUseCase,
    IPaymentsUseCase paymentsUseCase,
    IAlertService alertService,
    NavigationManager nav
    ) : AuthorizationComponentBase
{
    private EventUserAssignmentResponseModel? activeUserAssignment;
    private PaymentsResponseModel? payments;
    private string eventId = string.Empty;
    private bool isAdministrator;
    private Guid userId;
    private bool isLoading = true;
    private bool isProcessing = false;

    [SupplyParameterFromForm]
    private SettleEventRequestModel? SettleEventRequestModel { get; set; }

    protected override async Task OnInitializedAsync()
    {

        if (!await EnsureAuthorizedAsync())
        {
            return;
        }
        userId = await CurrentUserService.GetUserIdAsync();
        isAdministrator = IsAdmin;
        activeUserAssignment = await GetActiveUserAssignmentAsync();
        eventId = activeUserAssignment.EventId.ToString();
        payments = await GetPaymentsAsync();
        SettleEventRequestModel ??= new();
        isLoading = false;

    }

    private async Task<EventUserAssignmentResponseModel> GetActiveUserAssignmentAsync()
    {
        return await eventsUseCase.ExecuteGetUserAssignmentAsync(CurrentUserId, true);
    }

    private async Task<PaymentsResponseModel> GetPaymentsAsync()
    {
        return await paymentsUseCase.ExecuteAsync(Guid.Parse(eventId), true);
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
            var response = await eventsUseCase.ExecuteSettleEventAsync(requestModel);
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
