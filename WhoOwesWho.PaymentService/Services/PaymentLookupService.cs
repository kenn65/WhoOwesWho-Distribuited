using Mapster;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Repositories;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.Gateways;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IPaymentLookupService
    {
        Task<PaymentPageResponseModel> GetPaymentsPageDataAsync(PaymentsRequestModel request);
        Task<UserHasPaymentsResponseModel> GetUserPaymentsAsync(UserPaymentsRequestModel request);
        Task<PaymentDetailsPageResponseModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request);
        Task<PaymentDetailsPageResponseModel> GetSettlementDetailsAsync(SettlementDetailsRequestModel request);
    }

    public class PaymentLookupService(
        IConfiguration configuration,
        IPaymentCalculationService paymentCalculationService,
        IPaymentQueryRepository paymentQueryRepository,
        ICurrencyGatewayService currencyGatewayService,
        IPaymentCacheRepository paymentCacheRepository
        ) : ServiceBase(configuration), IPaymentLookupService
    {
        public async Task<PaymentPageResponseModel> GetPaymentsPageDataAsync(PaymentsRequestModel request)
        {
            var evt = await paymentCacheRepository.GetEventByIdAsync(request.EventId!, request.Active);

            if (evt is null && request.Active)
            {
                return new PaymentPageResponseModel
                {
                    Success = false,
                    Message = Constants.PaymentErrorMessages.PaymentsInavailable
                };
            }

            var allPayments = (await paymentQueryRepository.GetPaymentsAsync(request)).ToList();
            var eventUsers = await GetEventUsersAsync(evt!);
            var balances = (await paymentCalculationService.CalculateUserBalances(request, eventUsers))
                .OrderByDescending(a => a.Balance).ToList();
            var payments = (await paymentQueryRepository.GetPaymentsAsync(request)).ToList();

            var whoOwesWhoBalances = balances.Select(balance => new UserBalanceResponseModel
            {
                User = balance.User,
                Balance = balance.Balance,
                CurrencySymbol = balance.CurrencySymbol
            }).ToList();
            var whoOwesWho = (await paymentCalculationService.CalculateWhoOwesWho(whoOwesWhoBalances)).ToList();

            var eventModel = evt.Adapt<EventModel>();
            eventModel!.Users = eventUsers!;

            var response = new PaymentPageResponseModel
            {
                Event = eventModel,
                Payments = payments,
                Balances = balances,
                WhoOwesWho = whoOwesWho,
                Success = true
            };
            return response;
        }

        public async Task<PaymentDetailsPageResponseModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request)
        {
           
            var paymentDetails = await paymentQueryRepository.GetPaymentDetailsAsync(request);
            var evt = await paymentCacheRepository.GetEventByIdAsync(paymentDetails.EventId!, request.Active);
            var activeEvent = evt.Adapt<EventModel>();
            activeEvent.Users = await GetEventUsersAsync(evt!);
            var currencies = (await currencyGatewayService.GetCurrenciesAsync(request.Token!))?.Data;

            return new PaymentDetailsPageResponseModel()
            {
                PaymentDetails = paymentDetails,
                Event = activeEvent,
                Currencies = currencies,
                Success = true
            };

        }

        public async Task<PaymentDetailsPageResponseModel> GetSettlementDetailsAsync(SettlementDetailsRequestModel request)
        {

            var paymentDetails = await paymentQueryRepository.GetPaymentDetailsAsync(request);
            var evt = await paymentCacheRepository.GetEventByIdAsync(paymentDetails.EventId!, false);
            var inactiveEvent = evt.Adapt<EventModel>();
            inactiveEvent.Users = await GetEventUsersAsync(evt!);
            var currencies = (await currencyGatewayService.GetCurrenciesAsync(request.Token!))?.Data;

            return new PaymentDetailsPageResponseModel()
            {
                PaymentDetails = paymentDetails,
                Event = inactiveEvent,
                Currencies = currencies,
                Success = true
            };

        }

        private async Task<IEnumerable<UserModel>> GetEventUsersAsync(EventMessageResponseModel evt)
        {
            var users = (await Task.WhenAll(
                   evt.UserIds!.Select(async id =>
                   await paymentCacheRepository.GetUserByIdAsync(id)
                   ?? new UserMessageResponseModel()
                   ))).ToList();
            return [.. users.Select(user => user.Adapt<UserModel>())];
        }

        public async Task<UserHasPaymentsResponseModel> GetUserPaymentsAsync(UserPaymentsRequestModel request)
        {
            var response = new UserHasPaymentsResponseModel();
            var paymentsRequestModel = request.Adapt<PaymentsRequestModel>();
            var payments = await GetPaymentsPageDataAsync(paymentsRequestModel);
            var hasPayments = payments.Payments?.Any(p => p?.CreditEventUser?.Id == request.UserId ||
                p?.DebitEventUser?.Id == request.UserId);
            var success = hasPayments.HasValue && hasPayments.Value
                ? response.Success = true
                : response.Success = false;
            return response;
        }
    }
}
