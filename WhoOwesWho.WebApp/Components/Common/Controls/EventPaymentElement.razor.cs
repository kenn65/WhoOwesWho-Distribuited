using Microsoft.AspNetCore.Components;
using System.Globalization;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Common.Controls;

public partial class EventPaymentElement(
    NavigationManager nav,
    ICookiesMasterService cookiesMasterService, 
    IUserUseCase userUseCase,
    IAlertService alertService)
{
    [Parameter] public bool HasAssignment { get; set; }
    [Parameter] public bool HasPayments { get; set; }
    [Parameter] public bool IsProcessing { get; set; }
    [Parameter] public EventResponseModel? EventResponseModel { get; set; }
    [Parameter] public IEnumerable<CurrencyResponseModel>? CurrencyList { get; set; }
    [Parameter] public UserBalanceResponseModel? UserBalanceResponseModel { get; set; }
    [Parameter] public EventCallback<CreatePaymentRequestModel> HandlePayment { get; set; }
    [Parameter] public EventCallback<EventUnassignmentRequestModel> HandleUnassign { get; set; }

    [SupplyParameterFromForm(FormName = "unassign")]
    private EventUnassignmentRequestModel? EventUnassignmentRequestModel { get; set; }

    [SupplyParameterFromForm(FormName = "payment")]
    private CreatePaymentRequestModel? CreatePaymentRequestModel { get; set; } = null;
    private string EventIdAsString { get; set; } = string.Empty;
    private string PlaceHolder { get; set; } = string.Empty;

    private UserModel? CurrentUser { get; set; } 

    protected override async Task OnInitializedAsync()
    {
        EventUnassignmentRequestModel ??= new EventUnassignmentRequestModel();
        CreatePaymentRequestModel ??= new CreatePaymentRequestModel();
        EventIdAsString = EventResponseModel!.Id.ToString();
        EventResponseModel.Users = EventResponseModel.Users?.OrderBy(u => u?.FullName); 
        PlaceHolder = $"0{CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator}00";
        CurrentUser = await GetCurrentUser();

    }

    private async Task HandleUnassignSubmitAsync()
    {
        IsProcessing = true;
        if (EventUnassignmentRequestModel != null)
        {
            EventUnassignmentRequestModel.EventId = EventIdAsString;
            await HandleUnassign.InvokeAsync(EventUnassignmentRequestModel);
        }
    }

    private async Task<UserModel> GetCurrentUser()
    {
        var cookies = await cookiesMasterService.GetAsync();
        var response = await userUseCase.ExecuteAsync(cookies!.UserIdValue, cookies.TokenValue, false);
        if (!response.Success)
        {
            await alertService.Error("An unexpected error occurred, please try again");
            nav.NavigateTo("/me/workspace", true);
            return response;
        }
        return response;
        
    }

    private async Task HandlePaymentSubmit()
    {
        IsProcessing = true;
        if (CreatePaymentRequestModel != null)
        {
            CreatePaymentRequestModel.EventId = EventIdAsString;
            CreatePaymentRequestModel.OriginalCurrency = CreatePaymentRequestModel.Currency;
            CreatePaymentRequestModel.Currency = EventResponseModel!.Currency;
            await HandlePayment.InvokeAsync(CreatePaymentRequestModel);
        }
    }

    private async Task OnUsersSelected(IEnumerable<string> userIds)
    {
        CreatePaymentRequestModel!.UserIds = userIds;
        var test = CreatePaymentRequestModel;
    }
}
