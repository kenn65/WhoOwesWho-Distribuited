using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services.ServiceBus.Senders.User;
using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Encryption;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Event;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IUserBalanceService
    {
        public Task<UserBalanceResponseModel> GetUserBalanceAsync(UserBalanceRequestModel request, bool active);
    }

    public class UserBalanceService(
        IConfiguration configuration,
        IDataQueryService dataSelectionService,
        IProtectValueMessageSender protectValueMessageSender,
        IPaymentEventMessageSender paymentEventMessageSender,
        IPaymentUserMessageSender paymentUserMessageSender)
        : ServiceBase(configuration), IUserBalanceService
    {
        public async Task<UserBalanceResponseModel> GetUserBalanceAsync(UserBalanceRequestModel request, bool active)
        {
            try
            {
                var thisEvent = await paymentEventMessageSender.SendAsync(new SbEventRequestModel 
                { 
                    UserOrEventId = request.EventId!, 
                    Active = active 
                });
                
                var protectedUserId = await protectValueMessageSender.SendAsync(new ProtectValueRequestModel
                {
                    ApiKey = AppSettings.EncryptionMicroServiceApiKey!,
                    Text = request.UserId!
                });

                var userCredits = (await dataSelectionService.GetUserPaymentsAsync(request, true)).ToList();
                var userDebits = (await dataSelectionService.GetUserPaymentsAsync(request, false)).ToList();

                var creditUserAmountSum = userCredits.Any() ? userCredits.Sum(c => c.Amount) : 0;
                var debitUserAmountSum = userDebits.Any() ? userDebits.Sum(d => d.Amount) : 0;

                return await Task.FromResult(new UserBalanceResponseModel
                {
                    User = await paymentUserMessageSender.SendAsync(new UserRequestModel 
                    { 
                        ApiKey = AppSettings.UserMicroServiceApiKey!,
                        IdOrEmailAddress = protectedUserId, 
                        IncludePassword = false 
                    }),
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
