using System.Data;
using Microsoft.Data.SqlClient;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Services.Base;

namespace WhoOwesWho.PaymentService.Services
{
    public interface IDataMutationService
    {
        Task<CreatePaymentResponseModel> AddPaymentAsync(CreatePaymentRequestModel request, long timeTicks);
        Task<CreatePaymentResponseModel> AddPaymentUserAsync(CreatePaymentRequestModel request, long timeTicks, bool isCreditor);

        Task<UpdatePaymentResponseModel> UpdatePaymentAsync(UpdatePaymentRequestModel request);
        Task<DeletePaymentResponseModel> DeletePaymentAsync(DeletePaymentRequestModel request);
        Task<DeletePaymentResponseModel> DeletePaymentUsersAsync(DeletePaymentRequestModel request);

    }

    public class DataMutationService(IConfiguration configuration)
        : ServiceBase(configuration), IDataMutationService
    {
        public async Task<CreatePaymentResponseModel> AddPaymentAsync(CreatePaymentRequestModel request, long timeTicks)
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    await connection.OpenAsync();
                    var command =
                        new SqlCommand(
                            "INSERT INTO [WoW.Payments].[dbo].[WoW.Payment] ([Id], [EventId], [Amount], [TotalAmount], [Currency], [OriginalAmount], [OriginalCurrency], [Description], [Created], [CreditorIncluded]) VALUES (@id, @eventId, @amount, @totalAmount, @currency, @originalAmount, @originalCurrency, @description, @created, @creditorIncluded)",
                            connection);
                    command.Parameters.AddWithValue("@id", request.PaymentId);
                    command.Parameters.AddWithValue("@eventId", request.EventId);
                    command.Parameters.AddWithValue("@amount", request.Amount);
                    command.Parameters.AddWithValue("@totalAmount", request.TotalAmount);
                    command.Parameters.AddWithValue("@currency", request.Currency);
                    command.Parameters.AddWithValue("@originalAmount", request.OriginalAmount);
                    command.Parameters.AddWithValue("@originalCurrency", request.OriginalCurrency);
                    command.Parameters.AddWithValue("@description", request.Description);
                    command.Parameters.AddWithValue("@created", timeTicks);
                    command.Parameters.AddWithValue("@creditorIncluded", request.CreditorIncluded);
                    command.CommandType = CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                }

                return await Task.FromResult(new CreatePaymentResponseModel
                {
                    Success = true
                });
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
        }

        public async Task<CreatePaymentResponseModel> AddPaymentUserAsync(CreatePaymentRequestModel request, long timeTicks,
            bool isCreditor)
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    await connection.OpenAsync();
                    var command =
                        new SqlCommand(
                            "INSERT INTO [WoW.Payments].[dbo].[WoW.PaymentUsers] ([PaymentId], [UserId], [IsCreditor], [Created]) VALUES (@paymentId, @userId, @isCreditor, @created)",
                            connection);
                    command.Parameters.AddWithValue("@paymentId", request.PaymentId);
                    command.Parameters.AddWithValue("@userId", isCreditor ? request.CreditorId : request.DebitorId);
                    command.Parameters.AddWithValue("@isCreditor", isCreditor);
                    command.Parameters.AddWithValue("@created", timeTicks);
                    command.CommandType = CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                }

                return await Task.FromResult(new CreatePaymentResponseModel
                {
                    Success = true
                });
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
        }

        public async Task<UpdatePaymentResponseModel> UpdatePaymentAsync(UpdatePaymentRequestModel request)
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand(
                        "UPDATE [WoW.Payments].[dbo].[WoW.Payment] SET [Amount] = @amount, [TotalAmount] = @totalAmount, [OriginalAmount] = @originalAmount, [OriginalCurrency] = @originalCurrency, [Description] = @description, [CreditorIncluded] = @creditorIncluded WHERE[Id] = @paymentId",
                    connection);
                    command.Parameters.AddWithValue("@paymentId", request.PaymentId);
                    command.Parameters.AddWithValue("@amount", request.Amount);
                    command.Parameters.AddWithValue("@totalAmount", request.TotalAmount);
                    command.Parameters.AddWithValue("@originalAmount", request.OriginalAmount);
                    command.Parameters.AddWithValue("@originalCurrency", request.OriginalCurrency);
                    command.Parameters.AddWithValue("@description", request.Description);
                    command.Parameters.AddWithValue("@creditorIncluded", request.CreditorIncluded);
                    command.CommandType = CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                }

                return await Task.FromResult(new UpdatePaymentResponseModel
                {
                    Success = true,
                });
            }
            catch 
            {
                return await Task.FromResult(new UpdatePaymentResponseModel
                {
                    Success = false,
                });
            }
        }

        public async Task<DeletePaymentResponseModel> DeletePaymentAsync(DeletePaymentRequestModel request)
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    await connection.OpenAsync();
                    var deletePaymentUsersCommand =
                        new SqlCommand(
                            "DELETE FROM [WoW.Payments].[dbo].[WoW.PaymentUsers] WHERE [PaymentId] = @paymentId",
                            connection);
                    deletePaymentUsersCommand.Parameters.AddWithValue("@paymentId", request.PaymentId);
                    deletePaymentUsersCommand.CommandType = CommandType.Text;
                    await deletePaymentUsersCommand.ExecuteNonQueryAsync();

                    var deletePaymentCommand =
                        new SqlCommand("DELETE FROM [WoW.Payments].[dbo].[WoW.Payment] WHERE [Id] = @paymentId",
                            connection);
                    deletePaymentCommand.Parameters.AddWithValue("@paymentId", request.PaymentId);
                    deletePaymentUsersCommand.CommandType = CommandType.Text;
                    await deletePaymentUsersCommand.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                }
                
                return await Task.FromResult(new DeletePaymentResponseModel
                {
                    Success = true,
                    Message = "Payment deleted successfully."
                });
            }
            catch 
            {
                return await Task.FromResult(new DeletePaymentResponseModel
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please, try again."
                });
            }
        }

        public async Task<DeletePaymentResponseModel> DeletePaymentUsersAsync(DeletePaymentRequestModel request)
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    await connection.OpenAsync();
                    var deletePaymentUsersCommand =
                        new SqlCommand(
                            "DELETE FROM [WoW.Payments].[dbo].[WoW.PaymentUsers] WHERE [PaymentId] = @paymentId",
                            connection);
                    deletePaymentUsersCommand.Parameters.AddWithValue("@paymentId", request.PaymentId);
                    deletePaymentUsersCommand.CommandType = CommandType.Text;
                    await deletePaymentUsersCommand.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                }
                return await Task.FromResult(new DeletePaymentResponseModel
                {
                    Success = true,
                });
            }
            catch 
            {
                return await Task.FromResult(new DeletePaymentResponseModel
                {
                    Success = false,
                });
            }
        }
    }
}
