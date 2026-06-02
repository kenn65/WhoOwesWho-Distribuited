using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Components.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Payments;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Workspace;

public partial class Workspace(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    IAlertService alertService,
    ICurrenciesUseCase currenciesUseCase,
    IPaymentsUseCase paymentsUseCase
    ) : AuthorizationComponentBase
{
    private IEnumerable<EventResponseModel>? eventResponseModels;
    private EventResponseModel? eventResponseModel;
    private EventUserAssignmentResponseModel? eventUserAssignmentResponseModel;
    private IEnumerable<CurrencyResponseModel>? currencies;
    private UserBalanceResponseModel? userBalanceResponseModel;
    private bool hasAssignment = false;
    private bool hasPayments = false;
    private bool isProcessing = false;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        if (!await EnsureAuthorizedAsync())
        {
            return;
        }
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
        return await eventsUseCase.ExecuteGetEventsAsync(true);
    }

    private async Task<EventResponseModel> GetEventAsync(Guid eventId)
    {
        return await eventsUseCase.ExecuteGetEventAsync(eventId, true);
    }

    private async Task<EventUserAssignmentResponseModel> GetEventAssignmentAsync()
    {
        return await eventsUseCase.ExecuteGetUserAssignmentAsync(CurrentUserId, true);
    }
    private async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync()
    {
        return await currenciesUseCase.ExecuteAsync();
    }

    private async Task<bool> HasUserPaymentsAsync(string eventId)
    {
        var response = await paymentsUseCase.ExecuteAsync(Guid.Parse(eventId), CurrentUserId, true);
        return response.Success;
    }

    private async Task<UserBalanceResponseModel> GetUserBalanceAsync(string eventId)
    {
        return await paymentsUseCase.ExecuteAsync(CurrentUserId, Guid.Parse(eventId));
    }

    private async Task HandleAssignAsync(EventAssignmentRequestModel requestModel)
    {
        if (string.IsNullOrWhiteSpace(requestModel.EventIdString) || string.IsNullOrWhiteSpace(requestModel.EventIdString))
        {
            await StopProcessing();
            await alertService.Error("Please select an event!");
            return;
        }
        requestModel!.UserIdString = CurrentUserId.ToString();
        var response = await eventsUseCase.ExecuteAssignToEventAsync(requestModel);
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
        requestModel.UserIdString = CurrentUserId.ToString();
        var response = await eventsUseCase.ExecuteUnassignFromEventAsync(requestModel);
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
        requestModel.CreditorId = CurrentUserId;
        requestModel.OriginalAmount = requestModel.TotalAmount!.Value;
        var responnse = await paymentsUseCase.ExecuteAsync(requestModel);
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
