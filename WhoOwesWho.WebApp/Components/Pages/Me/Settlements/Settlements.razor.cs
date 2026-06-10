using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Components.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Payments;
namespace WhoOwesWho.WebApp.Components.Pages.Me.Settlements;

public partial class Settlements(
    IEventsUseCase eventsUseCase,
    IPaymentsUseCase paymentsUseCase,
    IAlertService alertService,
    NavigationManager nav
    ) : AuthorizationComponentBase
{
    private IEnumerable<EventResponseModel>? eventList;
    private PaymentsResponseModel? payments;
    private bool isAdministrator = false;
    private bool isProcessing = false;
    private bool isLoading = true;


    [SupplyParameterFromForm]
    private SettleEventRequestModel? SettleEventRequestModel { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!await EnsureAuthorizedAsync())
        {
            return;
        }
        isAdministrator = IsAdmin;
        eventList = await GetUserEventsAsync();
        SettleEventRequestModel ??= new();
        isLoading = false;
    }

    private async Task<IEnumerable<EventResponseModel>> GetUserEventsAsync()
    {
        return await eventsUseCase.ExecuteGetEventsAsync(false);
    }

    private async Task OnEventChangedAsync(string eventId)
    {
        SettleEventRequestModel?.EventIdString = eventId;
        payments = await GetPaymentsAsync(Guid.Parse(eventId));
    }

    private async Task<PaymentsResponseModel> GetPaymentsAsync(Guid eventId)
    {
        return await paymentsUseCase.ExecuteAsync(eventId, false);
    }

    private async Task HandleReopenEventAsync()
    {
        var confirmation = await alertService.Confirm("Are you sure you want to reopen this event?");
        if (confirmation)
        {
            isProcessing = true;
            var response = await eventsUseCase.ExecuteUnsettleEventAsync(SettleEventRequestModel!);
            await StopProcessingAsync();
            if (!response.Success)
            {
                await alertService.Error(response.Message!);
                return;
            }
            await alertService.Success(response.Message!);
            nav.NavigateTo("me/payments", true);
        }
    }
        
    private async Task StopProcessingAsync()
    {
        isProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}


