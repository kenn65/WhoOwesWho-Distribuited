using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.Gateways;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IPaymentCalculationService
    {
        Task<CalculateAmountResponseModel> CalculateAmount(CreatePaymentRequestModel request);
        Task<IEnumerable<UserBalanceResponseModel>> CalculateUserBalances(PaymentsRequestModel request, IEnumerable<UserModel> eventUsers);
        Task<IEnumerable<WhoOwesWhoModel>> CalculateWhoOwesWho(IEnumerable<UserBalanceResponseModel> balances);

        public class PaymentCalculationService(
            IConfiguration configuration,
            IUserBalanceService userBalanceService,
            ICurrencyGatewayService currencyGatewayService
            ) : ServiceBase(configuration), IPaymentCalculationService
        {
            public async Task<CalculateAmountResponseModel> CalculateAmount(CreatePaymentRequestModel request)
            {
                var exchangeRateResponse = await currencyGatewayService.GetExchangeRateAsync(request.OriginalCurrency!,
                    request.Currency!, request.Token!);

                var usersCount = request.UserIds!.Count();
                var totalAmount = request.OriginalAmount * exchangeRateResponse.ExchangeRate;
                return new CalculateAmountResponseModel
                {
                    TotalAmount = totalAmount,
                    Amount = totalAmount / usersCount,
                    Currency = request.Currency
                };
            }

            public async Task<IEnumerable<UserBalanceResponseModel>> CalculateUserBalances(PaymentsRequestModel request, IEnumerable<UserModel> eventUsers)
            {
                var balances = new List<UserBalanceResponseModel>();
                foreach (var user in eventUsers)
                {
                    var userBalanceRequest = new UserBalanceRequestModel
                    {
                        UserId = user.Id,
                        EventId = request.EventId
                    };
                    balances.Add(await userBalanceService.GetUserBalanceAsync(userBalanceRequest, request.Active));
                }

                return balances;
            }

            public async Task<IEnumerable<WhoOwesWhoModel>> CalculateWhoOwesWho(IEnumerable<UserBalanceResponseModel> balances)
            {
                var whoOwesWhoModels = new List<WhoOwesWhoModel>();
                var userBalances = balances.ToList();
                var creditorBalances = userBalances.Where(b => b.Balance > 0).ToList();
                var debitorBalances = userBalances.Where(b => b.Balance < 0).ToList();
                for (var d = debitorBalances.Count - 1; d > -1; d--)
                {
                    for (var c = creditorBalances.Count - 1; c > -1; c--)
                    {
                        if (creditorBalances[c].Balance == 0)
                        {
                            continue;
                        }

                        if (creditorBalances[c].Balance >= Math.Abs(debitorBalances[d].Balance!))
                        {
                            whoOwesWhoModels.Add(new WhoOwesWhoModel
                            {
                                CreditorName = creditorBalances[c].User?.FullName,
                                DebitorName = debitorBalances[d].User?.FullName,
                                Amount = Math.Abs(debitorBalances[d].Balance!)
                            });
                            creditorBalances[c].Balance -= Math.Abs(debitorBalances[d].Balance!);
                            debitorBalances[d].Balance = 0M;
                        }
                        else
                        {
                            whoOwesWhoModels.Add(new WhoOwesWhoModel
                            {
                                CreditorName = creditorBalances[c].User?.FullName,
                                DebitorName = debitorBalances[d].User?.FullName,
                                Amount = Math.Abs(creditorBalances[c].Balance!)
                            });
                            debitorBalances[d].Balance += creditorBalances[c].Balance;
                            creditorBalances[c].Balance = 0M;
                        }
                    }
                }
                return whoOwesWhoModels;
            }
        }
    }
}
