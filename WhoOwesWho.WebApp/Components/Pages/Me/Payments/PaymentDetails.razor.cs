using Mapster;
using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Components.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.StateHandlers;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Payments;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Payments;

public partial class PaymentDetails
(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    IAlertService alertService,
    ICurrenciesUseCase currenciesUseCase,
    IPaymentsUseCase paymentsUseCase,
    IStateHandler<PaymentStateModel> stateHandler
    ) : AuthorizationComponentBase

{
    private EventUserAssignmentResponseModel? eventUserAssignmentResponseModel;
    private EventResponseModel? eventResponseModel;
    private IEnumerable<CurrencyResponseModel>? currencies;
    private UserBalanceResponseModel? userBalanceResponseModel;
    private PaymentDetailsResponseModel? paymentDetailsResponseModel;
    private bool isAdministrator = false;
    private bool isProcessing = false;
    private Guid paymentId = Guid.Empty;
    private Guid creditUserId = Guid.Empty;
    private bool hasAssignment = false;
    private bool hasPayments = false;
    private bool isLoading = true;
    private bool active = false;

    protected override async Task OnInitializedAsync()
    {
        if (!await EnsureAuthorizedAsync())
        {
            return;
        }
        paymentId = stateHandler.SelectedItem!.PaymentId;
        creditUserId = stateHandler.SelectedItem.CreditUserId;
        active = stateHandler.SelectedItem.Active;
        isAdministrator = await CurrentUserService.GetIsAdminAsync();
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
        return await eventsUseCase.ExecuteGetEventAsync(eventId, active);
    }

    private async Task<EventUserAssignmentResponseModel> GetEventAssignmentAsync()
    {
        return await eventsUseCase.ExecuteGetUserAssignmentAsync(CurrentUserId, active);
    }
    private async Task<IEnumerable<CurrencyResponseModel>> GetCurrenciesAsync()
    {
        return await currenciesUseCase.ExecuteAsync();
    }

    private async Task<PaymentDetailsResponseModel> GetPaymentDetailsAsync()
    {
        return await paymentsUseCase.ExecuteAsync(active, paymentId);
    }
    private async Task<bool> HasUserPaymentsAsync(string eventId)
    {
        var response = await paymentsUseCase.ExecuteAsync(Guid.Parse(eventId), CurrentUserId, active);
        return response.Success;
    }
    private async Task<UserBalanceResponseModel> GetUserBalanceAsync(Guid eventId)
    {
        return await paymentsUseCase.ExecuteAsync(CurrentUserId, eventId);
    }

    private async Task HandleUpdateAsync(CreatePaymentRequestModel request)
    {
        var requestModel = request.Adapt<UpdatePaymentRequestModel>();
        requestModel.PaymentId = paymentId;
        requestModel.CreditorId = paymentDetailsResponseModel!.PaymentDetails!.CreditEventUser!.Id;
        requestModel.OriginalAmount = requestModel.TotalAmount!.Value;
        
        var response = await paymentsUseCase.ExecuteAsync(requestModel);
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
        var response = await paymentsUseCase.ExecuteAsync(paymentId);
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