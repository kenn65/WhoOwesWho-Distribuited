using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.StateHandlers;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Events;

public partial class EditEvent(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    IAlertService alertService,
    ICookiesMasterService cookiesMasterService,
    IUserUseCase userUseCase,
    ICurrenciesUseCase currenciesUseCase,
    IStateHandler<EventModel> eventState,
    IProtectionUseCase protectionUseCase)
{
    private IEnumerable<CurrencyResponseModel> CurrencyList = new List<CurrencyResponseModel>();
    private Guid EventId;
    private bool IsProcessing = false;
    private CookiesResponseModel? Cookies = null;
    private string minDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
    private Guid UserId { get; set; }
    private Dictionary<string, object> _dateAttributes => new()
    {
        { "min", minDate }
    };

    [SupplyParameterFromForm]
    public EventResponseModel? EventResponseModel { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Cookies = await cookiesMasterService.GetAsync();
        UserId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(Cookies!.UserIdValue));
        EventId = eventState.SelectedItem!.Id;
        EventResponseModel = await GetEventAsync();
        EventResponseModel.StartDateDate = new DateTime(EventResponseModel.StartDate);
        CurrencyList = await GetCurrenciesAsync();

    }

    private async Task<EventResponseModel> GetEventAsync()
    {
        return await eventsUseCase.ExecuteGetEventAsync(EventId, true, Cookies!.TokenValue);
    }

    private async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync()
    {
        return (await currenciesUseCase.ExecuteAsync(Cookies!.TokenValue))?.Data!;
    }

    private async Task HandleSubmit()
    {
        IsProcessing = true;
        var requestModel = new EventRequestModel
        {
            Id = EventId,
            Name = EventResponseModel!.Name,
            Location = EventResponseModel.Location,
            Currency = EventResponseModel.Currency,
            StartDate = EventResponseModel.StartDateDate.ToString("yyyy-MM-dd"),
            CreatedBy = EventResponseModel.CreatedBy,
            Token = Cookies!.TokenValue
        };

        var response = await eventsUseCase.ExecuteUpdateEventAsync(requestModel, Cookies!.TokenValue);
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
        var response = await userUseCase.ExecuteAsync(UserId, Cookies!.TokenValue);
        return response!.FullName!;
    }

    private async Task HandleCancel()
    {
        nav.NavigateTo("/me/events", true);
    }

    private async Task StopProcessing()
    {
        IsProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}
