using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Events;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Events;
public partial class CreateEvent(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    IAlertService alertService,
    ICookiesMasterService cookiesMasterService,
    IUserUseCase userUseCase,
    ICurrenciesUseCase currenciesUseCase)
{
    private string minDate = DateTime.Today.ToString("yyyy-MM-dd");
    private IEnumerable<CurrencyResponseModel> CurrencyList = [];
    private Dictionary<string, object> dateAttributes => new()
    {
        { "min", minDate }
    };
    private bool IsProcessing = false;
    private CookiesResponseModel? Cookies;

    [SupplyParameterFromForm]
    public EventRequestModel? EventRequestModel { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Cookies = await cookiesMasterService.GetAsync();
        EventRequestModel ??= new EventRequestModel();
        EventRequestModel.StartDateDate = DateTime.Today;
        EventRequestModel.CreatedBy = await GetUserAsync();
        CurrencyList = await GetCurrenciesAsync();
    }

    private async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync()
    {
        return await currenciesUseCase.GetCurrenciesAsync(Cookies!.TokenValue);
    }

    private async Task HandleSubmit()
    {
        IsProcessing = true;
        EventRequestModel!.StartDate = EventRequestModel!.StartDateDate.ToString("yyyy-MM-dd");
        EventRequestModel.UserId = Cookies!.UserIdValue;
        var response = await eventsUseCase.ExecuteAsync(EventRequestModel, Cookies!.TokenValue);
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
        var response = await userUseCase.ExecuteAsync(
                Cookies!.UserIdValue,
                Cookies.TokenValue,
                false);
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
