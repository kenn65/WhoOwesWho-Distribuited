using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Common.Controls;

public partial class EventPaymentElement(
    NavigationManager nav,
    ICookiesMasterService cookiesMasterService,
    IUserUseCase userUseCase,
    IAlertService alertService,
    IHostNameService hostNameService,
    IProtectionUseCase protectionUseCase)
{
    [Parameter] public bool HasAssignment { get; set; }
    [Parameter] public bool HasPayments { get; set; }
    [Parameter] public bool IsProcessing { get; set; }
    [Parameter] public EventResponseModel? EventResponseModel { get; set; }
    [Parameter] public IEnumerable<CurrencyResponseModel>? CurrencyList { get; set; }
    [Parameter] public UserBalanceResponseModel? UserBalanceResponseModel { get; set; }
    [Parameter] public PaymentDetailsResponseModel? PaymentDetailsResponseModel { get; set; }
    [Parameter] public EventCallback<CreatePaymentRequestModel> HandlePayment { get; set; }
    [Parameter] public EventCallback<CreatePaymentRequestModel> HandleUpdate { get; set; }
    [Parameter] public EventCallback HandleDelete { get; set; }

    [SupplyParameterFromForm]
    private CreatePaymentRequestModel? CreatePaymentRequestModel { get; set; }

    private string placeHolder = string.Empty;
    private bool isPaymentDetails;
    private bool isAdministrator;
    private UserModel? currentUser;
    private CookiesResponseModel? cookies;
    private Guid userId;

    protected override async Task OnInitializedAsync()
    {
        cookies = await cookiesMasterService.GetAsync();
        isAdministrator = await cookiesMasterService.IsAdministratorAsync(cookies!);
        var host = await hostNameService.GetAsync();
        userId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(cookies!.UserIdValue));
        isPaymentDetails = nav.Uri == $"https://{host}/me/payment/details";
        CreatePaymentRequestModel ??= new CreatePaymentRequestModel();
        EventResponseModel?.Users = EventResponseModel.Users?.OrderBy(u => u?.FullName);
        if (isPaymentDetails)
        {
            CreatePaymentRequestModel.UserIds = EventResponseModel?.Users!.Select(u => u!.Id.ToString());
        }
        else
        {
            CreatePaymentRequestModel.UserIds = null;
        }
        placeHolder = $"0{CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator}00";
        currentUser = await GetCurrentUser();

        if (isPaymentDetails)
        {
            await Populate(PaymentDetailsResponseModel!);
        }
    }

    private async Task<UserModel> GetCurrentUser()
    {
        var response = await userUseCase.ExecuteAsync(userId, cookies!.TokenValue, false);
        if (!response.Success)
        {
            await alertService.Error("An unexpected error occurred, please try again");
            nav.NavigateTo("/me/workspace", true);
            return response;
        }
        return response;

    }

    private async Task Populate(PaymentDetailsResponseModel payment)
    {
        CreatePaymentRequestModel!.TotalAmount = payment.PaymentDetails!.OriginalAmount!;
        CreatePaymentRequestModel!.UserIds = payment.PaymentDetails.DebitEventUserIds!;
        CreatePaymentRequestModel.Currency = payment.PaymentDetails.OriginalCurrency!;
        CreatePaymentRequestModel.Description = payment.PaymentDetails.Description!;
        CreatePaymentRequestModel.CreditorId = payment.PaymentDetails.CreditEventUser!.Id;
        StateHasChanged();
        await Task.Yield();
    }

    private async Task HandlePaymentSubmit()
    {
        IsProcessing = true;

        if (CreatePaymentRequestModel == null || !CreatePaymentRequestModel!.UserIds!.Any() || (CreatePaymentRequestModel!.UserIds!.Count() == 1 && CreatePaymentRequestModel.UserIds!.Any(u => u == userId.ToString())))
        {
            await alertService.Error("No users are checked or you cheked only yourself");
            nav.NavigateTo("/me/workspace", true);
            return;
        }

        CreatePaymentRequestModel!.EventId = EventResponseModel!.Id;
        CreatePaymentRequestModel.OriginalCurrency = CreatePaymentRequestModel.Currency;
        CreatePaymentRequestModel.Currency = EventResponseModel!.Currency;
        if (isPaymentDetails)
        {
            await HandleUpdate.InvokeAsync(CreatePaymentRequestModel);
        }

        await HandlePayment.InvokeAsync(CreatePaymentRequestModel);

    }

    private async Task HandleDeleteAsync()
    {
        await HandleDelete.InvokeAsync();
    }
}
