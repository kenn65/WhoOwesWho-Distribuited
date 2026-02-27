using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Repositories;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.Gateways;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IUserBalanceService
    {
        public Task<UserBalanceResponseModel> GetUserBalanceAsync(UserBalanceRequestModel request, bool active);
    }

    public class UserBalanceService(
        IConfiguration configuration,
        IPaymentQueryRepository paymentQueryRepository,
        IUserGatewayService userGatewayService,
        IEventGatewayService eventGatewayService,
        IEncryptionGatewayService encryptionGatewayService


        ) : ServiceBase(configuration), IUserBalanceService
    {
        public async Task<UserBalanceResponseModel> GetUserBalanceAsync(UserBalanceRequestModel request, bool active)
        {
            try
            {
                var thisEvent = await eventGatewayService.GetEventAsync(request.EventId!, request.Token!, true, active);
                var protectedUserId = await encryptionGatewayService.ProtectAsync(request.UserId!);
                var userCredits = (await paymentQueryRepository.GetUserPaymentsAsync(request, true)).ToList();
                var userDebits = (await paymentQueryRepository.GetUserPaymentsAsync(request, false)).ToList();
                
                var creditUserAmountSum = userCredits.Any() ? userCredits.Sum(c => c.Amount) : 0;
                var debitUserAmountSum = userDebits.Any() ? userDebits.Sum(d => d.Amount) : 0;

                return await Task.FromResult(new UserBalanceResponseModel
                {
                    User = await userGatewayService.GetAuthorizedUserAsync(protectedUserId, request.Token!, true,
                        false),
                    Balance = decimal.Round(creditUserAmountSum - debitUserAmountSum, 2, MidpointRounding.AwayFromZero),
                    CurrencySymbol = thisEvent.CurrencySymbol
                });
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
        }
    }
}
