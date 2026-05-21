using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Payments;
namespace WhoOwesWho.WebApp.Components.Pages.Me.Settlements;

public partial class Settlements(
    ICookiesMasterService cookiesMasterService,
    IEventsUseCase eventsUseCase,
    IPaymentsUseCase paymentsUseCase,
    IAlertService alertService,
    NavigationManager nav)
{
    private CookiesResponseModel? cookies;
    private IEnumerable<EventResponseModel>? eventList;
    private PaymentsResponseModel? payments;
    private bool isAdminisstrator = false;
    private bool isProcessing = false;
    private bool isLoading = true;


    [SupplyParameterFromForm]
    private SettleEventRequestModel? settleEventRequestModel { get; set; }


    protected override async Task OnInitializedAsync()
    {
        cookies = await cookiesMasterService.GetAsync();
        isAdminisstrator = await cookiesMasterService.IsAdministratorAsync(cookies!);
        eventList = await GetUserEventsAsync();
        settleEventRequestModel ??= new();
        isLoading = false;
    }

    private async Task<IEnumerable<EventResponseModel>> GetUserEventsAsync()
    {
        return await eventsUseCase.ExecuteGetEventsAsync(false, cookies!.TokenValue);
    }

    private async Task OnEventChangedAsync(string eventId)
    {
        settleEventRequestModel?.EventIdString = eventId;
        payments = await GetPaymentsAsync(Guid.Parse(eventId));
    }

    private async Task<PaymentsResponseModel> GetPaymentsAsync(Guid eventId)
    {
        return await paymentsUseCase.ExecuteAsync(eventId, false, cookies!.TokenValue);
    }

    private async Task HandleReopenEventAsync()
    {
        var confirmation = await alertService.Confirm("Are you sure you want to reopen this event?");
        if (confirmation)
        {
            isProcessing = true;
            var response = await eventsUseCase.ExecuteUnsettleEventAsync(settleEventRequestModel!, cookies!.TokenValue);
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


