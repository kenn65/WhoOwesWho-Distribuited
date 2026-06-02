using Microsoft.AspNetCore.Components;
using System.Diagnostics.Eventing.Reader;
using WhoOwesWho.WebApp.Components.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.StateHandlers;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Events;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Events;

public partial class EditEvent(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    IAlertService alertService,
    ICurrenciesUseCase currenciesUseCase,
    IStateHandler<EventModel> eventState) : AuthorizationComponentBase
{
    private IEnumerable<CurrencyResponseModel> currencyList = [];
    private Guid eventId;
    private bool isProcessing = false;
    private readonly string minDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
    private Dictionary<string, object> DateAttributes => new()
    {
        { "min", minDate }
    };

    [SupplyParameterFromForm]
    public EventResponseModel? EventResponseModel { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!await EnsureAuthorizedAsync())
        {
            return;
        }
        eventId = eventState.SelectedItem!.Id;
        EventResponseModel = await GetEventAsync();
        EventResponseModel.StartDateDate = new DateTime(EventResponseModel.StartDate);
        currencyList = await GetCurrenciesAsync();
    }

    private async Task<EventResponseModel> GetEventAsync()
    {
        return await eventsUseCase.ExecuteGetEventAsync(eventId, true);
    }

    private async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync()
    {
        return await currenciesUseCase.ExecuteAsync();
    }

    private async Task HandleSubmit()
    {
        isProcessing = true;
        var requestModel = new EventRequestModel
        {
            Id = eventId,
            Name = EventResponseModel!.Name,
            Location = EventResponseModel.Location,
            Currency = EventResponseModel.Currency,
            StartDate = EventResponseModel.StartDateDate.ToString("yyyy-MM-dd"),
            CreatedBy = EventResponseModel.CreatedBy,
        };

        var response = await eventsUseCase.ExecuteUpdateEventAsync(requestModel);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            return;
        }
        await alertService.Success(response.Message!);
        nav.NavigateTo($"/me/events", true);
    }

    private async Task StopProcessing()
    {
        isProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}
