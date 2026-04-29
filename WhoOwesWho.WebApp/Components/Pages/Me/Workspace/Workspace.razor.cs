using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Payments;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Workspace;

public partial class Workspace(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    ICookiesMasterService cookiesMasterService,
    IAlertService alertService,
    ICurrenciesUseCase currenciesUseCase,
    IPaymentsUseCase paymentsUseCase)
{
    private IEnumerable<EventResponseModel>? EventResponseModels { get; set; }
    private EventResponseModel? EventResponseModel { get; set; }
    private EventUserAssignmentResponseModel? EventUserAssignmentResponseModel { get; set; }
    private IEnumerable<CurrencyResponseModel>? Currencies { get; set; }
    private UserBalanceResponseModel? UserBalanceResponseModel { get; set; }

    private CookiesResponseModel? Cookies = null;
    private bool HasAssignment = false;
    private bool HasPayments = false;
    private bool IsProcessing = false;

    protected override async Task OnInitializedAsync()
    {
        Cookies = await cookiesMasterService.GetAsync();
        EventUserAssignmentResponseModel = await GetEventAssignmentAsync();
        HasAssignment = EventUserAssignmentResponseModel!.EventId != Guid.Empty;
        if (!HasAssignment)
        {
            EventResponseModels = await GetEventsAsync();
        }
        else
        {
            HasPayments = await HasUserPaymentsAsync(EventUserAssignmentResponseModel!.EventId.ToString());
            Currencies = await GetCurrenciesAsync();
            EventResponseModel = await GetEventAsync(EventUserAssignmentResponseModel!.EventId);
            UserBalanceResponseModel = await GetUserBalanceAsync(EventUserAssignmentResponseModel!.EventId.ToString());
        }
    }

    private async Task OnAssignAsync(EventAssignmentRequestModel requestModel)
    {
        IsProcessing = true;
        await HandleAssignAsync(requestModel);
    }

    private async Task OnUnassignAsync(EventUnassignmentRequestModel requestModel)
    {
        IsProcessing = true;
        await HandleUnassingAsync(requestModel);
    }

    private async Task OnPaymentAsync(CreatePaymentRequestModel requestModel)
    {
        IsProcessing = true;
        await HandlePaymentAsync(requestModel);
    }

    private async Task<IEnumerable<EventResponseModel>> GetEventsAsync()
    {
        return await eventsUseCase.ExecuteAsync(true, Cookies!.TokenValue);
    }

    private async Task<EventResponseModel> GetEventAsync(Guid eventId)
    {
        return await eventsUseCase.ExecuteAsync(eventId.ToString(), true, Cookies!.TokenValue);
    }

    private async Task<EventUserAssignmentResponseModel> GetEventAssignmentAsync()
    {
        return await eventsUseCase.ExecuteAsync(Cookies!.UserIdValue, Cookies.TokenValue);
    }
    private async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync()
    {
        return await currenciesUseCase.GetCurrenciesAsync(Cookies!.TokenValue);
    }

    private async Task<bool> HasUserPaymentsAsync(string eventId)
    {
        var response = await paymentsUseCase.ExecuteAsync(eventId, Cookies!.UserIdValue, true, Cookies.TokenValue);
        return response.Success;
    }

    private async Task<UserBalanceResponseModel> GetUserBalanceAsync(string eventId)
    {
        return await paymentsUseCase.ExecuteAsync(Cookies!.UserIdValue, eventId, Cookies.TokenValue);
    }

    private async Task HandleAssignAsync(EventAssignmentRequestModel requestModel)
    {
        if (requestModel.EventId == null || requestModel.EventId == string.Empty)
        {
            await StopProcessing();
            await alertService.Error("Please select an event!");
            return;
        }
        requestModel!.UserId = Cookies!.UserIdValue;
        var response = await eventsUseCase.ExecuteAsync(requestModel, Cookies.TokenValue);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            return;
        }
        await alertService.Success(response.Message!);
        nav.NavigateTo("/me/workspace", true);
    }

    private async Task HandleUnassingAsync(EventUnassignmentRequestModel requestModel)
    {
        if (requestModel.EventId is null || requestModel.EventId == string.Empty)
        {
            await StopProcessing();
            await alertService.Error("An unexpected error occurred, please try again");
            return;
        }
        requestModel.UserId = Cookies!.UserIdValue;
        var response = await eventsUseCase.ExecuteAsync(requestModel, Cookies.TokenValue);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            return;
        }
        await alertService.Success(response.Message!);
        nav.NavigateTo("/me/workspace", true);
    }

    private async Task HandlePaymentAsync(CreatePaymentRequestModel requestModel)
    {
        if (requestModel.UserIds == null || !requestModel.UserIds.Any() || requestModel.UserIds.Count() < 2)
        {
            await StopProcessing();
            await alertService.Error("Please select at least two users!");
            return;
        }
        if (requestModel.EventId == null || requestModel.EventId == string.Empty)
        {
            await StopProcessing();
            await alertService.Error("An unexpected error with the event ID occurred, please try again");
            return;
        }
        if (requestModel.TotalAmount == null)
        {
            await StopProcessing();
            await alertService.Error("Please enter an payment amount");
            return;
        }
        if (string.IsNullOrWhiteSpace(requestModel.Description))
        {
            await StopProcessing();
            await alertService.Error("Please enter a description");
            return;
        }
        requestModel.CreditorId = Cookies!.UserIdValue;
        requestModel.OriginalAmount = requestModel.TotalAmount!.Value;
        requestModel.Token = Cookies.TokenValue;
        var responnse = await paymentsUseCase.ExecuteAsync(requestModel, Cookies!.TokenValue);
        await StopProcessing();
        if (!responnse.Success)
        {
            await alertService.Error(responnse.Message!);
            return;
        }
        await alertService.Success(responnse.Message!);
        nav.NavigateTo("/me/workspace", true);
    }






    private async Task StopProcessing()
    {
        IsProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }


}
