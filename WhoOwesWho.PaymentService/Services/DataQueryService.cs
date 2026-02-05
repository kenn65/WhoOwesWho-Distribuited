using Microsoft.Data.SqlClient;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.Models.Models;
using WhoOwesWho.Models.Models.Extensions;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services.Base;
using WhoOwesWho.PaymentService.Services.Gateways;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IDataQueryService
    {
        Task<IEnumerable<UserPaymentModel>> GetUserPaymentsAsync(UserBalanceRequestModel request, bool I);
        Task<IEnumerable<UserPaymentModel>> GetPaymentsAsync(PaymentsRequestModel request);
        Task<PaymentDetailsModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request);
    }

    public class DataQueryService(
        IConfiguration configuration,
        IEncryptionGatewayService encryptionGatewayService,
        IUserGatewayService userGatewayService
        ) : ServiceBase(configuration), IDataQueryService
    {
        public async Task<IEnumerable<UserPaymentModel>> GetUserPaymentsAsync(UserBalanceRequestModel request,
            bool isCreditor)
        {
            try
            {
                var creditor = isCreditor ? 1 : 0;
                var userPaymentModels = new List<UserPaymentModel>();
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    await connection.OpenAsync();
                    var command =
                        new SqlCommand(
                            "SELECT p.[Id], p.[EventId], p.[Amount], p.[Currency], p.[OriginalAmount], p.[OriginalCurrency] FROM [WoW.Payments].[dbo].[WoW.Payment] p INNER JOIN [WoW.Payments].[dbo].[WoW.PaymentUsers] pu ON p.[Id] = pu.[PaymentId] WHERE p.[EventId] = @eventId AND pu.[UserId] = @userId AND pu.[IsCreditor] = @isCreditor",
                            connection);
                    command.Parameters.AddWithValue("@eventId", request.EventId);
                    command.Parameters.AddWithValue("@userId", request.UserId);
                    command.Parameters.AddWithValue("@isCreditor", creditor);
                    command.CommandType = System.Data.CommandType.Text;
                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        userPaymentModels.Add(new UserPaymentModel
                        {
                            Id = reader.GetGuid(0),
                            EventId = reader.GetGuid(1),
                            Amount = reader.GetDecimal(2),
                            Currency = reader.GetString(3),
                            OriginalAmount = reader.GetDecimal(4),
                            OriginalCurrency = reader.GetString(5)
                        });
                    }

                    await reader.CloseAsync();
                    await connection.CloseAsync();
                }

                return await Task.FromResult(userPaymentModels);
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
        }

        public async Task<IEnumerable<UserPaymentModel>> GetPaymentsAsync(PaymentsRequestModel request)
        {
            var userPaymentModels = new List<UserPaymentModel>();

            try
            {
                await using var connection = new SqlConnection(AppSettings.DatabaseConnectionString);
                await connection.OpenAsync();
                var command =
                    new SqlCommand(
                        "SELECT p.[Id], p.[EventId], p.[Amount], p.[Currency], p.[OriginalAmount], p.[OriginalCurrency], p.[Description], p.[Created], pu.[UserId], pu.[IsCreditor] FROM [WoW.Payments].[dbo].[WoW.Payment] p INNER JOIN [WoW.Payments].[dbo].[WoW.PaymentUsers] pu ON p.[Id] = pu.[PaymentId] WHERE p.[EventId] = @eventId ORDER By pu.[Created]",
                        connection);
                command.Parameters.AddWithValue("@eventId", request.EventId);
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (reader.GetBoolean(9))
                    {
                        var protectedUserId =
                            await encryptionGatewayService.ProtectAsync(reader.GetGuid(8).ToString());

                        userPaymentModels.Add(new UserPaymentModel
                        {
                            Id = reader.GetGuid(0),
                            EventId = reader.GetGuid(1),
                            Amount = reader.GetDecimal(2),
                            Currency = reader.GetString(3),
                            OriginalAmount = reader.GetDecimal(4),
                            OriginalCurrency = reader.GetString(5),
                            Description = reader.GetString(6),
                            Created = new DateTime(reader.GetInt64(7)).ToDisplayDateTimeFormat(),
                            CreditEventUser =
                                await userGatewayService.GetAuthorizedUserAsync(protectedUserId, request.Token!,
                                    true, false)

                        });
                    }
                    else
                    {
                        var protectedUserId =
                           await encryptionGatewayService.ProtectAsync(reader.GetGuid(8).ToString());

                        var paymentId = reader.GetGuid(0);
                        var userPaymentModel = userPaymentModels.LastOrDefault(i => i.Id == paymentId);
                        if (userPaymentModel != null)
                        {
                            userPaymentModel.DebitEventUser =
                                await userGatewayService.GetAuthorizedUserAsync(protectedUserId, request.Token!,
                                    true,
                                    false);
                        }
                    }
                }

                await reader.CloseAsync();
                await connection.CloseAsync();
                return userPaymentModels;
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }

        }

        public async Task<PaymentDetailsModel> GetPaymentDetailsAsync(PaymentDetailsPageRequestModel request)
        {
            try
            {
                var response = new PaymentDetailsModel();
                var debitorEventUsers = new List<UserModel>();
                await using var connection = new SqlConnection(AppSettings.DatabaseConnectionString);
                await connection.OpenAsync();
                var command =
                    new SqlCommand(
                        "SELECT DISTINCT p.[Id], p.[EventId], p.[Amount], p.[Currency], p.[OriginalAmount], p.[OriginalCurrency], p.[Description], p.[Created], p.[CreditorIncluded], pu.[UserId], pu.[IsCreditor] FROM [WoW.Payments].[dbo].[WoW.Payment] p INNER JOIN [WoW.Payments].[dbo].[WoW.PaymentUsers] pu ON p.[Id] = pu.[PaymentId] WHERE p.[id] = @paymentId ORDER By pu.[IsCreditor] DESC ",
                        connection);
                command.Parameters.AddWithValue("@paymentId", request.PaymentId);
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (reader.GetBoolean(10))
                    {
                        var protectedUserId = 
                            await encryptionGatewayService.ProtectAsync(reader.GetGuid(9).ToString());

                        response.PaymentId = reader.GetGuid(0).ToString();
                        response.EventId = reader.GetGuid(1).ToString();
                        response.Amount = reader.GetDecimal(2);
                        response.Currency = reader.GetString(3);
                        response.OriginalAmount = reader.GetDecimal(4);
                        response.OriginalCurrency = reader.GetString(5);
                        response.Description = reader.GetString(6);
                        response.Created = new DateTime(reader.GetInt64(7)).ToDisplayDateTimeFormat();
                        response.CreditorIncluded = reader.GetBoolean(8);
                        response.CreditEventUser =
                            await userGatewayService.GetAuthorizedUserAsync(protectedUserId, request.Token!, true,
                                false);
                    }
                    else
                    {
                        var protectedUserId =
                            await encryptionGatewayService.ProtectAsync(reader.GetGuid(9).ToString());

                        var debitEventUser = await userGatewayService.GetAuthorizedUserAsync(protectedUserId,
                            request.Token!, true,
                            false);

                        debitorEventUsers.Add(debitEventUser);
                    }
                }

                await reader.CloseAsync();
                await connection.CloseAsync();
                response.DebitEventUsers = debitorEventUsers;
                return await Task.FromResult(response);
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
        }
    }
}
