using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections.Abstractions;
using System.Net;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.StateHandlers;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Payments;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Payments;

public partial class PaymentDetails
(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    ICookiesMasterService cookiesMasterService,
    IAlertService alertService,
    ICurrenciesUseCase currenciesUseCase,
    IPaymentsUseCase paymentsUseCase,
    IStateHandler<PaymentStateModel> stateHandler,
    IProtectionUseCase protectionUseCase
    )

{
    private EventUserAssignmentResponseModel? eventUserAssignmentResponseModel { get; set; }
    private EventResponseModel? eventResponseModel { get; set; }
    private IEnumerable<CurrencyResponseModel>? currencies { get; set; }
    private UserBalanceResponseModel? userBalanceResponseModel { get; set; }
    private PaymentDetailsResponseModel? paymentDetailsResponseModel { get; set; }
    private bool isAdministrator { get; set; }

    private CookiesResponseModel? cookies = null;
    private bool isProcessing = false;
    private Guid paymentId = Guid.Empty;
    private Guid creditUserId = Guid.Empty;
    private Guid userId = Guid.Empty;
    private bool hasAssignment = false;
    private bool hasPayments = false;
    private bool isLoading = true;
    private bool active = false;

    protected override async Task OnInitializedAsync()
    {
        paymentId = stateHandler.SelectedItem!.PaymentId;
        creditUserId = stateHandler.SelectedItem.CreditUserId;
        active = stateHandler.SelectedItem.Active;
        cookies = await cookiesMasterService.GetAsync();
        isAdministrator = await cookiesMasterService.IsAdministratorAsync(cookies!);
        userId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(cookies!.UserIdValue));
        eventUserAssignmentResponseModel = await GetEventAssignmentAsync();
        eventResponseModel = await GetEventAsync(eventUserAssignmentResponseModel!.EventId);
        hasAssignment = eventUserAssignmentResponseModel!.EventId != Guid.Empty;
        hasPayments = await HasUserPaymentsAsync(eventUserAssignmentResponseModel!.EventId.ToString());
        currencies = await GetCurrenciesAsync();
        userBalanceResponseModel = await GetUserBalanceAsync(eventUserAssignmentResponseModel!.EventId);
        paymentDetailsResponseModel = await GetPaymentDetailsAsync();
        isLoading = false;
    }
    
    private async Task OnUpdateAsync(CreatePaymentRequestModel requestModel)
    {
        isProcessing = true;
        await HandleUpdateAsync(requestModel);
    }

    private async Task OnDeleteAsync()
    {
        var confirmation = await alertService.Confirm("Are you sure you want to delete this payment?");
        if (confirmation)
        {
            await HandleDeleteAsync();
        }
    }

    private async Task<EventResponseModel> GetEventAsync(Guid eventId)
    {
        return await eventsUseCase.ExecuteGetEventAsync(eventId, active, cookies!.TokenValue);
    }

    private async Task<EventUserAssignmentResponseModel> GetEventAssignmentAsync()
    {
        return await eventsUseCase.ExecuteGetUserAssignmentAsync(userId, active, cookies!.TokenValue);
    }
    private async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync()
    {
        return (await currenciesUseCase.ExecuteAsync(cookies!.TokenValue))?.Data!;
    }

    private async Task<PaymentDetailsResponseModel> GetPaymentDetailsAsync()
    {
        return await paymentsUseCase.ExecuteAsync(active, paymentId, cookies!.TokenValue);
    }
    private async Task<bool> HasUserPaymentsAsync(string eventId)
    {
        var response = await paymentsUseCase.ExecuteAsync(Guid.Parse(eventId), userId, active, cookies!.TokenValue);
        return response.Success;
    }
    private async Task<UserBalanceResponseModel> GetUserBalanceAsync(Guid eventId)
    {
        return await paymentsUseCase.ExecuteAsync(userId, eventId, cookies!.TokenValue);
    }

    private async Task HandleUpdateAsync(CreatePaymentRequestModel request)
    {
        var requestModel = request.Adapt<UpdatePaymentRequestModel>();
        requestModel.PaymentId = paymentId;
        requestModel.CreditorId = paymentDetailsResponseModel!.PaymentDetails!.CreditEventUser!.Id;
        requestModel.OriginalAmount = requestModel.TotalAmount!.Value;
        requestModel.Token = cookies!.TokenValue;
        var response = await paymentsUseCase.ExecuteAsync(requestModel, cookies!.TokenValue);
        await StopProcessingAsync();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            nav.NavigateTo("/me/payment/details", true);
            return;
        }
        await alertService.Success(response.Message!);
        nav.NavigateTo("me/payments", true);
    }

    private async Task HandleDeleteAsync()
    {
        var response = await paymentsUseCase.ExecuteAsync(cookies!.TokenValue, paymentId);
        await StopProcessingAsync();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            nav.NavigateTo("/me/payment/details", true);
            return;
        }
        await alertService.Success(response.Message!);
        nav.NavigateTo("me/payments", true);
    }


    private async Task StopProcessingAsync()
    {
        isProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}