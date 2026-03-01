using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Repositories;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.Gateways;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IPaymentLookupService
    {
        Task<PaymentPageResponseModel> GetPaymentsPageDataAsync(PaymentsRequestModel request);
        Task<PaymentDetailsPageResponseModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request);
        Task<PaymentDetailsPageResponseModel> GetSettlementDetailsAsync(SettlementDetailsRequestModel request);
    }

    public class PaymentLookupService(
        IConfiguration configuration, 
        IPaymentSecurityService paymentSecurityService, 
        IPaymentCalculationService paymentCalculationService, 
        IPaymentQueryRepository paymentQueryRepository, 
        IEventGatewayService eventGatewayService,
        ICurrencyGatewayService currencyGatewayService
        ) : ServiceBase(configuration), IPaymentLookupService
    {
        public async Task<PaymentPageResponseModel> GetPaymentsPageDataAsync(PaymentsRequestModel request)
        {
            try
            {
                var thisEvent = request.EventId is null
                    ? await eventGatewayService.GetUserEventAsync(request.UserId!, request.Token!, true,
                        request.Active)
                    : await eventGatewayService.GetEventAsync(request.EventId!, request.Token!, true,
                        request.Active);

                request.EventId = thisEvent.Id.ToString();
                var eventUsers =
                    (await eventGatewayService.GetEventUsersAsync(request.EventId, request.Token!, true, request.Active))
                    .ToList();
                var balances = (await paymentCalculationService.CalculateUserBalances(request, eventUsers)).OrderByDescending(a => a.Balance)
                    .ToList();
                var payments = (await paymentQueryRepository.GetPaymentsAsync(request)).ToList();


                for (var i = payments.Count - 1; i > -1; i--)
                {
                    var payment = payments[i];

                    payment.ProtectedPaymentId = await paymentSecurityService.ProtectAsync(payment.Id.ToString());
                    payment.ProtectedCreditUserId =
                        await paymentSecurityService.ProtectAsync(payment.CreditEventUser!.Id.ToString());
                }

                var whoOwesWhoBalances = balances.Select(balance => new UserBalanceResponseModel
                {
                    User = balance.User,
                    Balance = balance.Balance,
                    CurrencySymbol = balance.CurrencySymbol
                }).ToList();
                var whoOwesWho = (await paymentCalculationService.CalculateWhoOwesWho(whoOwesWhoBalances)).ToList();

                var response = new PaymentPageResponseModel
                {
                    Event = thisEvent,
                    Payments = payments,
                    Balances = balances,
                    WhoOwesWho = whoOwesWho
                };
                return await Task.FromResult(response);
            }
            catch (Exception e)
            {
                var test = e.Message;
                return new PaymentPageResponseModel
                {
                    Success = false,
                    Message = "No payments available. You are not assigned to an event. That my be because that the event has been settled (closed)."
                };
            }
        }

        public async Task<PaymentDetailsPageResponseModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request)
        {
            var paymentDetails = await paymentQueryRepository.GetPaymentDetailsAsync(request);
            var activeEvent = await eventGatewayService.GetEventAsync(paymentDetails.EventId!, request.Token!, true, true);
            activeEvent.Users = await eventGatewayService.GetEventUsersAsync(activeEvent.Id.ToString(), request.Token!, true, true);
            var currencies = await currencyGatewayService.GetCurrenciesAsync(request.Token!);

            return new PaymentDetailsPageResponseModel()
            {
                PaymentDetails = paymentDetails,
                Event = activeEvent,
                Currencies = currencies
            };
        }

        public async Task<PaymentDetailsPageResponseModel> GetSettlementDetailsAsync(SettlementDetailsRequestModel request)
        {
            var paymentDetails = await paymentQueryRepository.GetPaymentDetailsAsync(request);
            var activeEvent = await eventGatewayService.GetEventAsync(paymentDetails.EventId!, request.Token!, true, false);
            activeEvent.Users = await eventGatewayService.GetEventUsersAsync(activeEvent.Id.ToString(), request.Token!, true, false);
            var currencies = await currencyGatewayService.GetCurrenciesAsync(request.Token!);

            return new PaymentDetailsPageResponseModel()
            {
                PaymentDetails = paymentDetails,
                Event = activeEvent,
                Currencies = currencies
            };
        }
    }
}
