using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Components.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Events;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Events;

public partial class CreateEvent(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    IAlertService alertService,
    IUserUseCase userUseCase,
    ICurrenciesUseCase currenciesUseCase
    ) : AuthorizationComponentBase
{
    private readonly string minDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
    private IEnumerable<CurrencyResponseModel> CurrencyList = [];
    private Dictionary<string, object> DateAttributes => new()
    {
        { "min", minDate }
    };
    private bool isProcessing = false;
    
    [SupplyParameterFromForm]
    public EventRequestModel? EventRequestModel { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!await EnsureAuthorizedAsync())
        {
            return;
        }
        EventRequestModel ??= new EventRequestModel();
        EventRequestModel.StartDateDate = DateTime.Today;
        CurrencyList = await GetCurrenciesAsync();
        EventRequestModel.CreatedBy = await GetUserAsync();
    }

    private async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync()
    {
        return await currenciesUseCase.ExecuteAsync();
    }

    private async Task HandleSubmit()
    {
        isProcessing = true;
        EventRequestModel!.StartDate = EventRequestModel!.StartDateDate.ToString("yyyy-MM-dd");
        EventRequestModel.UserId = CurrentUserId;

        var response = await eventsUseCase.ExecuteCreateEventAsync(EventRequestModel);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            return;
        }
        await alertService.Success(response.Message!);
        nav.NavigateTo($"/me/events", true);
    }

    private async Task<string> GetUserAsync()
    {
        var response = await userUseCase.ExecuteAsync(CurrentUserId, false);
        return response!.FullName!;
    }

    private async Task StopProcessing()
    {
        isProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}
