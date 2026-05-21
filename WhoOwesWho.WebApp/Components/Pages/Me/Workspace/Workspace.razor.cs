using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Payments;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Workspace;

public partial class Workspace(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    ICookiesMasterService cookiesMasterService,
    IAlertService alertService,
    ICurrenciesUseCase currenciesUseCase,
    IPaymentsUseCase paymentsUseCase,
    IProtectionUseCase protectionUseCase)
{
    private IEnumerable<EventResponseModel>? eventResponseModels; 
    private EventResponseModel? eventResponseModel; 
    private EventUserAssignmentResponseModel? eventUserAssignmentResponseModel; 
    private IEnumerable<CurrencyResponseModel>? currencies;
    private UserBalanceResponseModel? userBalanceResponseModel; 
    private CookiesResponseModel? cookies = null;
    private bool hasAssignment = false;
    private bool hasPayments = false;
    private bool isProcessing = false;
    private Guid userId = Guid.Empty;
    private bool isLoading = true;
    
    protected override async Task OnInitializedAsync()
    {
        cookies = await cookiesMasterService.GetAsync();
        userId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(cookies!.UserIdValue));
        eventUserAssignmentResponseModel = await GetEventAssignmentAsync();
        hasAssignment = eventUserAssignmentResponseModel!.EventId != Guid.Empty;
        if (!hasAssignment)
        {
            eventResponseModels = await GetEventsAsync();
        }
        else
        {
            hasPayments = await HasUserPaymentsAsync(eventUserAssignmentResponseModel!.EventId.ToString());
            currencies = await GetCurrenciesAsync();
            eventResponseModel = await GetEventAsync(eventUserAssignmentResponseModel!.EventId);
            userBalanceResponseModel = await GetUserBalanceAsync(eventUserAssignmentResponseModel!.EventId.ToString());
        }
        isLoading = false;
    }

    private async Task OnAssignAsync(EventAssignmentRequestModel requestModel)
    {
        isProcessing = true;
        await HandleAssignAsync(requestModel);
    }

    private async Task OnUnassignAsync(EventUnassignmentRequestModel requestModel)
    {
        isProcessing = true;
        await HandleUnassingAsync(requestModel);
    }

    private async Task OnPaymentAsync(CreatePaymentRequestModel requestModel)
    {
        isProcessing = true;
        await HandlePaymentAsync(requestModel);
    }

    private async Task<IEnumerable<EventResponseModel>> GetEventsAsync()
    {
        return await eventsUseCase.ExecuteGetEventsAsync(true, cookies!.TokenValue);
    }

    private async Task<EventResponseModel> GetEventAsync(Guid eventId)
    {
        return await eventsUseCase.ExecuteGetEventAsync(eventId, true, cookies!.TokenValue);
    }

    private async Task<EventUserAssignmentResponseModel> GetEventAssignmentAsync()
    {
        return await eventsUseCase.ExecuteGetUserAssignmentAsync(userId, true, cookies!.TokenValue);
    }
    private async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync()
    {
        return (await currenciesUseCase.ExecuteAsync(cookies!.TokenValue))?.Data!;
    }

    private async Task<bool> HasUserPaymentsAsync(string eventId)
    {
        var response = await paymentsUseCase.ExecuteAsync(Guid.Parse(eventId), userId, true, cookies!.TokenValue);
        return response.Success;
    }

    private async Task<UserBalanceResponseModel> GetUserBalanceAsync(string eventId)
    {
        return await paymentsUseCase.ExecuteAsync(userId, Guid.Parse(eventId), cookies!.TokenValue);
    }

    private async Task HandleAssignAsync(EventAssignmentRequestModel requestModel)
    {
        if (string.IsNullOrWhiteSpace(requestModel.EventIdString) || string.IsNullOrWhiteSpace(requestModel.EventIdString))
        {
            await StopProcessing();
            await alertService.Error("Please select an event!");
            return;
        }
        requestModel!.UserIdString = userId.ToString();
        var response = await eventsUseCase.ExecuteAssignToEventAsync(requestModel, cookies!.TokenValue);
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
        if (requestModel.EventId == Guid.Empty)
        {
            await StopProcessing();
            await alertService.Error("An unexpected error occurred, please try again");
            return;
        }
        requestModel.UserIdString = userId.ToString();
        var response = await eventsUseCase.ExecuteUnassignFromEventAsync(requestModel, cookies!.TokenValue);
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
        requestModel.CreditorId = userId;
        requestModel.OriginalAmount = requestModel.TotalAmount!.Value;
        requestModel.Token = cookies!.TokenValue;
        var responnse = await paymentsUseCase.ExecuteAsync(requestModel, cookies!.TokenValue);
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
        isProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }


}
