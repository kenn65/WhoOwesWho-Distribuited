using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Events;
public partial class CreateEvent(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    IAlertService alertService,
    ICookiesMasterService cookiesMasterService,
    IUserUseCase userUseCase,
    ICurrenciesUseCase currenciesUseCase,
    IProtectionUseCase protectionUseCase
    )
{
    private string minDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
    private IEnumerable<CurrencyResponseModel> CurrencyList = [];
    private Dictionary<string, object> dateAttributes => new()
    {
        { "min", minDate }
    };
    private bool IsProcessing = false;
    private CookiesResponseModel? Cookies;
    private Guid UserId { get; set; }

    [SupplyParameterFromForm]
    public EventRequestModel? EventRequestModel { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Cookies = await cookiesMasterService.GetAsync();
        UserId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(Cookies!.UserIdValue));
        EventRequestModel ??= new EventRequestModel();
        EventRequestModel.StartDateDate = DateTime.Today;
        CurrencyList = await GetCurrenciesAsync();
        EventRequestModel.CreatedBy = await GetUserAsync();
    }

    private async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync()
    {
        return (await currenciesUseCase.ExecuteAsync(Cookies!.TokenValue))?.Data!;
    }

    private async Task HandleSubmit()
    {
        IsProcessing = true;
        EventRequestModel!.StartDate = EventRequestModel!.StartDateDate.ToString("yyyy-MM-dd");
        EventRequestModel.UserId = UserId;
        EventRequestModel.Token = Cookies!.TokenValue;

        var response = await eventsUseCase.ExecuteCreateEventAsync(EventRequestModel, Cookies!.TokenValue);
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
