using WhoOwesWho.EventService.Models;
using WhoOwesWho.Models.Models;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.Gateways;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IPaymentService
    {
        Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request);
        Task<PaymentPageResponseModel> GetPaymentsPageDataAsync(PaymentsRequestModel request);
    }

    public class PaymentService(
        IConfiguration configuration,
        IDataQueryService dataSelectionService,
        IDataMutationService dataModificationService,
        IUserBalanceService userBalanceService,
        IEncryptionGatewayService encryptionGatewayService,
        IEventGatewayService eventGatewayService,
        ICurrencyGatewayService currencyGatewayService
        ) : ServiceBase(configuration), IPaymentService
    {
        public async Task<CreatePaymentResponseModel> CreatePaymentAsync(CreatePaymentRequestModel request)
        {
            var timeTicks = DateTime.Now.Ticks;
            var amountCalculation = await CalculateAmount(request);
            request.Amount = amountCalculation.Amount;
            request.Currency = amountCalculation.Currency;
            request.TotalAmount = amountCalculation.TotalAmount;
            request.PaymentId = Guid.NewGuid();
            request.CreditorIncluded = request.UserIds!.Any(u => u == request.CreditorId);

            if (request.CreditorIncluded && request.UserIds!.Count() == 1)
            {
                return await Task.FromResult(new CreatePaymentResponseModel
                {
                    Message = "Payment invalid as the only debtor is yourself, which does not make sense."
                });
            }

            var paymentAddition = await dataModificationService.AddPaymentAsync(request, timeTicks);
            if (!paymentAddition.Success)
            {
                await CreateUnsuccessfulPaymentResponseAsync();
            }

            foreach (var userId in request.UserIds!.Where(u => u != request.CreditorId))
            {
                timeTicks = new DateTime(timeTicks).AddMicroseconds(100).Ticks;
                var creditUserAddition = await dataModificationService.AddPaymentUserAsync(request, timeTicks, true);
                if (!creditUserAddition.Success)
                {
                    return await CreateUnsuccessfulPaymentResponseAsync();
                }

                timeTicks = new DateTime(timeTicks).AddMicroseconds(100).Ticks;
                request.DebitorId = userId;
                var debitUserAddition = await dataModificationService.AddPaymentUserAsync(request, timeTicks, false);
                if (!debitUserAddition.Success)
                {
                    return await CreateUnsuccessfulPaymentResponseAsync();
                }
            }

            return await Task.FromResult(new CreatePaymentResponseModel()
            {
                Message = "Payment added successfully.",
                Success = true
            });
        }

        public async Task<PaymentPageResponseModel> GetPaymentsPageDataAsync(PaymentsRequestModel request)
        {
            try
            {
                var thisEvent = request.EventId == null
                    ? await eventGatewayService.GetUserEventAsync(request.UserId!, request.Token!, true,
                        request.Active)
                    : await eventGatewayService.GetEventAsync(request.EventId!, request.Token!, true,
                        request.Active);

                request.EventId = thisEvent.Id.ToString();
                var eventUsers =
                    (await eventGatewayService.GetEventUsersAsync(request.EventId, request.Token!, true, request.Active))
                    .ToList();
                var balances = (await CalculateUserBalances(request, eventUsers)).OrderByDescending(a => a.Balance)
                    .ToList();
                var payments = (await dataSelectionService.GetPaymentsAsync(request)).ToList();


                for (var i = payments.Count - 1; i > -1; i--)
                {
                    var payment = payments[i];

                    payment.ProtectedPaymentId = await encryptionGatewayService.ProtectAsync(payment.Id.ToString());
                    payment.ProtectedCreditUserId =
                        await encryptionGatewayService.ProtectAsync(payment.CreditEventUser!.Id.ToString());
                }

                var whoOwesWhoBalances = balances.Select(balance => new UserBalanceResponseModel
                {
                    User = balance.User,
                    Balance = balance.Balance,
                    CurrencySymbol = balance.CurrencySymbol
                }).ToList();
                var whoOwesWho = (await CalculateWhoOwesWho(whoOwesWhoBalances)).ToList();

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
                return await Task.FromResult(new PaymentPageResponseModel
                {
                    Success = false,
                    Message = "No payments available. You are not assigned to an event. That my be because that the event has been settled (closed)."
                });
            }
        }





        private async Task<IEnumerable<UserBalanceResponseModel>> CalculateUserBalances(PaymentsRequestModel request,
            IEnumerable<UserModel> eventUsers)
        {
            var balances = new List<UserBalanceResponseModel>();
            foreach (var user in eventUsers)
            {
                var userBalanceRequest = new UserBalanceRequestModel
                {
                    UserId = user.Id.ToString(),
                    EventId = request.EventId,
                    Token = request.Token
                };
                balances.Add(await userBalanceService.GetUserBalanceAsync(userBalanceRequest, request.Active));
            }

            return await Task.FromResult(balances);
        }

        private static async Task<IEnumerable<WhoOwesWhoModel>> CalculateWhoOwesWho(IEnumerable<UserBalanceResponseModel> balances)
        {
            var whoOwesWhoModels = new List<WhoOwesWhoModel>();
            // ReSharper disable once PossibleMultipleEnumeration
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

                    if (creditorBalances[c].Balance >= Math.Abs(debitorBalances[d].Balance))
                    {
                        whoOwesWhoModels.Add(new WhoOwesWhoModel
                        {
                            CreditorName = creditorBalances[c].User?.FullName,
                            DebitorName = debitorBalances[d].User?.FullName,
                            Amount = Math.Abs(debitorBalances[d].Balance)
                        });
                        creditorBalances[c].Balance -= Math.Abs(debitorBalances[d].Balance);
                        debitorBalances[d].Balance = 0M;
                    }
                    else
                    {
                        whoOwesWhoModels.Add(new WhoOwesWhoModel
                        {
                            CreditorName = creditorBalances[c].User?.FullName,
                            DebitorName = debitorBalances[d].User?.FullName,
                            Amount = Math.Abs(creditorBalances[c].Balance)
                        });
                        debitorBalances[d].Balance += creditorBalances[c].Balance;
                        creditorBalances[c].Balance = 0M;
                    }
                }
            }
            return await Task.FromResult(whoOwesWhoModels);
        }

        private async Task<CalculateAmountResponseModel> CalculateAmount(CreatePaymentRequestModel request)
        {
            var activeEventResponse = await eventGatewayService.GetEventAsync(request.EventId!, request.Token!, true, true);
            var exchangeRateResponse = await currencyGatewayService.GetExchangeRateAsync(request.OriginalCurrency!,
                activeEventResponse.Currency!, request.Token!);

            var usersCount = request.UserIds!.Count();
            var totalAmount = request.OriginalAmount * exchangeRateResponse.ExchangeRate;
            return await Task.FromResult(new CalculateAmountResponseModel
            {
                TotalAmount = totalAmount,
                Amount = totalAmount / usersCount,
                Currency = activeEventResponse.Currency
            });
        }

        private static async Task<CreatePaymentResponseModel> CreateUnsuccessfulPaymentResponseAsync()
        {
            return await Task.FromResult(new CreatePaymentResponseModel
            {
                Success = false,
                Message = "An unexpected error occurred. Please, try again."
            });
        }
    }
}
