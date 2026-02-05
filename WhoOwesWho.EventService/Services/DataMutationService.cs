using Microsoft.Data.SqlClient;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EventService.Services
{
    public interface IDataMutationService
    {
        Task<EventResponseModel?> CreateEventAsync(EventRequestModel request);
        Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request);
        Task<DeleteEventResponseModel> DeleteEventAsync(Guid id);
        Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request);
        Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request);
        Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request);


    }

    public class DataMutationService(
        IConfiguration configuration,
        IDataQueryService dataQueryService,
        IEncryptionGatewayService encryptionGatewayService,
        IUserGatewayService userGatewayService,
        ICurrencyGatewayService currencyGatewayService
        ) : ServiceBase(configuration), IDataMutationService
    {
        public async Task<EventResponseModel?> CreateEventAsync(EventRequestModel request)
        {
            try
            {
                request.Id = Guid.NewGuid();

                var creationUser = await userGatewayService.GetAuthorizedUserAsync(request.UserId!, request.Token!, true);

                if (string.IsNullOrWhiteSpace(creationUser!.FullName))
                {
                    return new EventResponseModel
                    {
                        Message = $"User Id: {request.UserId} was not found"
                    };
                }

                request.CurrencySymbol =
                   await currencyGatewayService.GetCurrencySymbolAsync(request.Currency!, request.Token!);

                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "INSERT INTO [WoW.Events].[dbo].[WoW.Event] (Id, CreatedBy, Name, Location, Currency, CurrencySymbol, StartDate, Settled) VALUES (@id, @createdBy, @name, @location, @currency, @currencySymbol, @startDate, @settled)",
                        connection);
                    command.Parameters.AddWithValue("@id", request.Id);
                    command.Parameters.AddWithValue("@createdBy", creationUser.FullName);
                    command.Parameters.AddWithValue("@name", request.Name);
                    command.Parameters.AddWithValue("@location", request.Location);
                    command.Parameters.AddWithValue("@currency", request.Currency);
                    command.Parameters.AddWithValue("@currencySymbol", request.CurrencySymbol);
                    command.Parameters.AddWithValue("@startDate", request.StartDateTicks);
                    command.Parameters.AddWithValue("@settled", request.Settled);
                    command.CommandType = System.Data.CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }

                if (request.AutoAssign)
                {
                    await AssignToEventAsync(new AssignmentRequestModel
                    {
                        EventId = request.Id.ToString(),
                        User = creationUser,
                        Token = request.Token
                    });
                }

                var response = await dataQueryService.GetEventAsync(request.Id, request.Token!, true);

                response!.Success = true;
                response.Message = "The event was successfully created.";

                return await Task.FromResult(response);
            }
            catch (Exception)
            {
                return new EventResponseModel
                {
                    Message = "An unexpected error occurred. Please, try again."
                };
            }
        }

        public async Task<UpdateResponseModel> UpdateEventAsync(EventRequestModel request)
        {
            var response = new UpdateResponseModel();
            try
            {
                await currencyGatewayService.GetCurrencySymbolAsync(request.Currency!, request.Token!);

                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "UPDATE [WoW.Events].[dbo].[WoW.Event] SET [Name] = @name, [Location] = @location, [Currency] = @currency, [CurrencySymbol] = @currencySymbol , [StartDate] = @startDate WHERE [Id] = @id",
                        connection);
                    command.Parameters.AddWithValue("@id", request.Id);

                    command.Parameters.AddWithValue("@name", request.Name);
                    command.Parameters.AddWithValue("@location", request.Location);
                    command.Parameters.AddWithValue("@currency", request.Currency);
                    command.Parameters.AddWithValue("@currencySymbol", request.CurrencySymbol);
                    command.Parameters.AddWithValue("@startDate", request.StartDateTicks);
                    command.CommandType = System.Data.CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }

                response.Success = true;
                response.Message = "The event was successfully updated.";

            }
            catch (Exception)
            {
                response.Message = "An unexpected error occurred. P=lease, try again.";
            }

            return await Task.FromResult(response);
        }

        public async Task<DeleteEventResponseModel> DeleteEventAsync(Guid id)
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "DELETE FROM [WoW.Events].[dbo].[WoW.Event] WHERE Id = @id",
                        connection);
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();

                    command = new SqlCommand(
                        "DELETE FROM [WoW.Events].[dbo].[WoW.EventAssignment] WHERE EventId = @eventId",
                        connection);
                    command.Parameters.AddWithValue("@eventId", id);
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }

                return new DeleteEventResponseModel
                {
                    Success = true,
                    Message = "Event was successfully removed."
                };
            }
            catch (Exception)
            {
                return new DeleteEventResponseModel
                {
                    Message = $"An unexpected error occurred. Please, try again."
                };
            }
        }

        public async Task<AssignmentResponseModel> AssignToEventAsync(AssignmentRequestModel request)
        {
            var response = new AssignmentResponseModel();
            try
            {
                request.User ??= await userGatewayService.GetAuthorizedUserAsync(request.UserId!, request.Token!, false, true);

                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "INSERT INTO [WoW.Events].[dbo].[WoW.EventAssignment] (EventId, UserId) VALUES (@eventId, @userId)",
                        connection);
                    command.Parameters.AddWithValue("@eventId", request.EventId);
                    command.Parameters.AddWithValue("@userId", request!.User!.Id);
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }
                var thisEvent = await dataQueryService.GetEventAsync(Guid.Parse(request.EventId!), request.Token!, true);

                response.Success = true;
                response.Message = $"You successfully assigned to event: {thisEvent?.Name} ({thisEvent?.Location}).";
            }
            catch (Exception)
            {
                {
                    response.Message = $"An unexpected error occurred. Please, try again.";
                }
            }

            return await Task.FromResult(response);
        }

        public async Task<UnassignmentResponseModel> UnassignFromEventAsync(UnassignmentRequestModel request)
        {
            var response = new UnassignmentResponseModel();
            try
            {
                var userId = await encryptionGatewayService.UnprotectAsync(request.UserId!);

                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "DELETE FROM [WoW.Events].[dbo].[WoW.EventAssignment] WHERE EventId = @eventId AND UserId = @userId",
                        connection);
                    command.Parameters.AddWithValue("@eventId", request.EventId);
                    command.Parameters.AddWithValue("@userId", userId);
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }
                var thisEvent = await dataQueryService.GetEventAsync(Guid.Parse(request.EventId!), request.Token!, true);
                response.Success = true;
                response.Message = $"You successfully unassigned from event: {thisEvent?.Name} ({thisEvent?.Location}).";
            }
            catch (Exception)
            {
                {
                    response.Message = $"An unexpected error occurred. Please, try again.";
                }
            }
            return await Task.FromResult(response);
        }

        public async Task<SettleEventResponseModel> SettleEventAsync(SettleEventRequestModel request)
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand(
                        "UPDATE [WoW.Events].[dbo].[WoW.Event] SET [Settled] = @settled WHERE [Id] = @eventId",
                        connection);
                    command.Parameters.AddWithValue("@eventId", request.EventId);
                    command.Parameters.AddWithValue("@settled", true);
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }

                return await Task.FromResult(new SettleEventResponseModel
                {
                    Success = true,
                    Message = "Event successfully settled."
                });
            }
            catch (Exception)
            {
                {
                    return await Task.FromResult(new SettleEventResponseModel
                    {
                        Success = false,
                        Message = "An unexpected error occurred. Please, try again"
                    });
                }
            }
        }
    }
}