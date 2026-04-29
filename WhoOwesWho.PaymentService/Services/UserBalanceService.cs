using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Repositories;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.Shared.Extensions;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IUserBalanceService
    {
        public Task<UserBalanceResponseModel> GetUserBalanceAsync(UserBalanceRequestModel request, bool active);
    }

    public class UserBalanceService(
        IConfiguration configuration,
        IPaymentSecurityService paymentSecurityService,
        IPaymentQueryRepository paymentQueryRepository,
        IPaymentCacheRepository paymentCacheRepository
        ) : ServiceBase(configuration), IUserBalanceService
    {
        public async Task<UserBalanceResponseModel> GetUserBalanceAsync(UserBalanceRequestModel request, bool active)
        {
            try
            {
                request.UserId = await paymentSecurityService.UnprotectAsync(request.UserId!);

                var thisEvent = await paymentCacheRepository.GetEventByIdAsync(request.EventId!, active); 
                var userCredits = (await paymentQueryRepository.GetUserPaymentsAsync(request, true)).ToList();
                var userDebits = (await paymentQueryRepository.GetUserPaymentsAsync(request, false)).ToList();
                
                var creditUserAmountSum = userCredits.Any() ? userCredits.Sum(c => c.Amount) : 0;
                var debitUserAmountSum = userDebits.Any() ? userDebits.Sum(d => d.Amount) : 0;
                
                return new UserBalanceResponseModel
                {
                    User = await paymentCacheRepository.GetUserByIdAsync(request.UserId!),
                    Balance = decimal.Round(creditUserAmountSum!.Value - debitUserAmountSum!.Value, 2, MidpointRounding.AwayFromZero),
                    CurrencySymbol = thisEvent!.CurrencySymbol
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
        }
    }
}
