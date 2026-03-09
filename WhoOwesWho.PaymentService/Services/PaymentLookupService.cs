using Mapster;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Repositories;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.Gateways;
using WhoOwesWho.Shared.Models;

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
        ICurrencyGatewayService currencyGatewayService,
        IPaymentCacheRepository paymentCacheRepository
        ) : ServiceBase(configuration), IPaymentLookupService
    {
        public async Task<PaymentPageResponseModel> GetPaymentsPageDataAsync(PaymentsRequestModel request)
        {
            try
            {
                request.EventId = request.EventId is null 
                    ? null 
                    : await paymentSecurityService.UnprotectAsync(request.EventId!);

                var evt = await paymentCacheRepository.GetEventByIdAsync(request.EventId!, request.Active);
                
                if (evt is null && request.Active)
                {
                    return new PaymentPageResponseModel
                    {
                        Success = false,
                        Message = "No payments available. Maybe the event has been settled (closed)."
                    };
                }
                                
                var allPayments = (await paymentQueryRepository.GetPaymentsAsync(request)).ToList();
                if (allPayments is null || !allPayments.Any())
                {
                    return new PaymentPageResponseModel
                    {
                        Success = false,
                        Message = "No payments has been made just yet."
                    };
                }

                var eventUsers = await GetEventUsersAsync(evt!);
                
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

                var eventModel = evt.Adapt<EventModel>();
                eventModel.Users = eventUsers!;
               
                var response = new PaymentPageResponseModel
                {
                    Event = eventModel,
                    Payments = payments,
                    Balances = balances,
                    WhoOwesWho = whoOwesWho
                };
                return response;
            }
            catch (Exception e)
            {
                var test = e.Message;
                return new PaymentPageResponseModel
                {
                    Success = false,
                    Message = "No payments available. You are not assigned to an active event. That my be because that the event has been settled (closed)."
                };
            }
        }

        public async Task<PaymentDetailsPageResponseModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request)
        {
            var paymentDetails = await paymentQueryRepository.GetPaymentDetailsAsync(request);
            var evt = await paymentCacheRepository.GetEventByIdAsync(paymentDetails.EventId!, true);
            var activeEvent = evt.Adapt<EventModel>();
            activeEvent.Users = await GetEventUsersAsync(evt!);
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
            var evt = await paymentCacheRepository.GetEventByIdAsync(paymentDetails.EventId!, false);
            var inactiveEvent = evt.Adapt<EventModel>();
            inactiveEvent.Users = await GetEventUsersAsync(evt!);
            var currencies = await currencyGatewayService.GetCurrenciesAsync(request.Token!);

            return new PaymentDetailsPageResponseModel()
            {
                PaymentDetails = paymentDetails,
                Event = inactiveEvent,
                Currencies = currencies
            };
        }

        private async Task<IEnumerable<UserModel>> GetEventUsersAsync(EventMessageResponseModel evt)
        {
            var users = (await Task.WhenAll(
                   evt.UserIds!.Select(async id =>
                   await paymentCacheRepository.GetUserByIdAsync(id.ToString())
                   ?? new UserMessageResponseModel()
                   ))).ToList();
            return users.Select(user => user.Adapt<UserModel>()).ToList();
            
        }

    }
}
