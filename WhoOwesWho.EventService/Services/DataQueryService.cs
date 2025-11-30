using System.Data;
using Microsoft.Data.SqlClient;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services.Base;
using WhoOwesWho.EventService.Services.ServiceBus.Senders.Encryption;
using WhoOwesWho.EventService.Services.ServiceBus.Senders.User;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EventService.Services
{
    public interface IDataQueryService
    {
        Task<EventResponseModel?> GetEventAsync(Guid id, bool active = true);

        Task<EventResponseModel?> GetEventByUserAsync(string userId, bool active = true);

        Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active = true);

        Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, 
            bool active = true);

        Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, bool active = true);
    }

    public class DataQueryService(
        IConfiguration configuration,
        IProtectValueMessageSender protectValueMessageSender, 
        IUnprotectValueMessageSender unprotectValueMessageSender, 
        IEventUserMessageSender eventUserEventService) : ServiceBase(configuration), IDataQueryService
    {
        public async Task<EventResponseModel?> GetEventAsync(Guid id, bool active = true)
        {

            EventResponseModel? response = null;
            var users = new List<UserModel>();

            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    var settled = active ? 0 : 1;
                    await connection.OpenAsync();
                    var command = new SqlCommand(
                        $"SELECT * FROM [WoW.Events].[dbo].[WoW.Event] WHERE Id = @id AND Settled = {settled}",
                        connection);
                    command.Parameters.AddWithValue("@id", id);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        response = new EventResponseModel
                        {
                            Id = reader.GetGuid(0),
                            CreatedBy = reader.GetString(1),
                            Name = reader.GetString(2),
                            Location = reader.GetString(3),
                            Currency = reader.GetString(4),
                            CurrencySymbol = reader.GetString(5),
                            StartDate = reader.GetInt64(6),
                            Settled = reader.GetBoolean(7)
                        };
                    }

                    await reader.CloseAsync();

                    command = new SqlCommand(
                        $"SELECT [UserId] FROM [dbo].[WoW.EventAssignment] WHERE [EventId] = @eventId",
                        connection);
                    command.Parameters.AddWithValue("@eventId", response?.Id);
                    reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        var protectedUserId = await protectValueMessageSender.SendAsync(new ProtectValueRequestModel
                        {
                            ApiKey = AppSettings.EncryptionMicroServiceApiKey!,
                            Text = reader.GetGuid(0).ToString()
                        });

                        var user = await eventUserEventService.SendAsync(new UserRequestModel
                        {
                            ApiKey = AppSettings.UserMicroServiceApiKey!,
                            IdOrEmailAddress = protectedUserId,
                            IncludePassword = true
                        });
                                                
                        users.Add(user);
                    }

                    await reader.CloseAsync();
                    await connection.CloseAsync();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }

            response!.Users = users;
            return await Task.FromResult(response);
        }

        public async Task<EventResponseModel?> GetEventByUserAsync(string userId, bool active = true)
        {
            var unprotectedUserId = Guid.Parse(await unprotectValueMessageSender.SendAsync(new UnprotectValueRequestModel
            {
                ApiKey = AppSettings.EncryptionMicroServiceApiKey!,
                Text = userId
            }));
            
            EventResponseModel eventResponseModel = null!;
            var users = new List<UserModel>();
            var settled = active ? 0 : 1;
            await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    $"SELECT e.[Id], e.[CreatedBy], e.[Name], e.[Location], e.[Currency], e.[CurrencySymbol], e.[StartDate], e.[Settled], ea.[UserId] FROM  [WoW.Events].[dbo].[WoW.Event] e INNER JOIN [WoW.Events].[dbo].[WoW.EventAssignment] ea ON e.Id = ea.EventId WHERE UserId = @userId AND Settled = {settled}",
                    connection);
                command.Parameters.AddWithValue("@userId", unprotectedUserId);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {

                    eventResponseModel = new EventResponseModel
                    {
                        Id = reader.GetGuid(0),
                        CreatedBy = reader.GetString(1),
                        Name = reader.GetString(2),
                        Location = reader.GetString(3),
                        Currency = reader.GetString(4),
                        CurrencySymbol = reader.GetString(5),
                        StartDate = reader.GetInt64(6),
                        Settled = reader.GetBoolean(7),
                    };

                    var user = await eventUserEventService.SendAsync(new UserRequestModel
                    {
                        ApiKey = AppSettings.UserMicroServiceApiKey!,
                        IdOrEmailAddress = userId,
                        IncludePassword = false
                    });
                    users.Add(user);
                }

                await reader.CloseAsync();
                await connection.CloseAsync();
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (eventResponseModel != null)
                {
                    eventResponseModel.Users = users;
                }

            }

            return await Task.FromResult(eventResponseModel);
        }

        public async Task<IEnumerable<EventResponseModel>> GetEventsAsync(bool active = true)
        {
            var response = new List<EventResponseModel>();

            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    var settled = active ? 0 : 1;
                    connection.Open();
                    var command = new SqlCommand(
                        $"SELECT * FROM [WoW.Events].[dbo].[WoW.Event] WHERE Settled = {settled}",
                        connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        var model = new EventResponseModel()
                        {
                            Id = reader.GetGuid(0),
                            CreatedBy = reader.GetString(1),
                            Name = reader.GetString(2),
                            Location = reader.GetString(3),
                            Currency = reader.GetString(4),
                            CurrencySymbol = reader.GetString(5),
                            StartDate = reader.GetInt64(6),
                            Settled = reader.GetBoolean(7),
                        };
                        response.Add(model);
                    }

                    await reader.CloseAsync();

                    foreach (var item in response)
                    {
                        var users = new List<UserModel>();
                        command = new SqlCommand(
                            $"SELECT [UserId] FROM [dbo].[WoW.EventAssignment] WHERE [EventId] = @eventId",
                            connection);
                        command.Parameters.AddWithValue("@eventId", item.Id);
                        reader = await command.ExecuteReaderAsync();
                        while (reader.Read())
                        {
                            var protectedUserId = await protectValueMessageSender.SendAsync(new ProtectValueRequestModel
                            {
                                ApiKey = AppSettings.EncryptionMicroServiceApiKey!,
                                Text = reader.GetGuid(0).ToString()
                            });

                            var user = await eventUserEventService.SendAsync(new UserRequestModel
                            {
                                ApiKey = AppSettings.UserMicroServiceApiKey!,
                                IdOrEmailAddress = protectedUserId, 
                                IncludePassword = true 
                            });
                            users.Add(user);
                        }
                        
                        await reader.CloseAsync();
                        item.Users = users;
                    }

                    await connection.CloseAsync();
                }
                
                return await Task.FromResult(response);
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
        }

        public async Task<EventAssignmentModel> GetAssignmentAsync(string protectedUserId, bool active = true)
        {
            EventAssignmentModel? response = null;
            try
            {
                var settled = active ? 0 : 1;
                var userId = Guid.Parse(await unprotectValueMessageSender.SendAsync(new UnprotectValueRequestModel
                {
                    ApiKey = AppSettings.EncryptionMicroServiceApiKey!,
                    Text = protectedUserId
                }));
                                
                await using var connection = new SqlConnection(AppSettings.DatabaseConnectionString);
                connection.Open();
                var command = new SqlCommand(
                    $"SELECT [EA].[EventId], [EA].[UserId] FROM [dbo].[WoW.EventAssignment] AS ea INNER JOIN [WoW.Events].[dbo].[WoW.Event] AS e ON [EA].[EventId] = [E].[Id] WHERE [EA].[UserId] = @userId AND [E].[Settled] = {settled}",
                    connection);
                command.Parameters.AddWithValue("@userId", userId.ToString());

                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var user = await eventUserEventService.SendAsync(new UserRequestModel
                    {
                        ApiKey = AppSettings.UserMicroServiceApiKey!,
                        IdOrEmailAddress = protectedUserId,
                        IncludePassword = false
                    });
                    
                    response = new EventAssignmentModel
                    {
                        EventId = reader.GetGuid(0),
                        User = user
                    };
                }

                await reader.CloseAsync();
                response ??= new EventAssignmentModel();
                return await Task.FromResult(response);
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
        }

        public async Task<IEnumerable<UserModel>> GetEventUsersAsync(string eventId, bool active = true)
        {
            try
            {
                var userModels = new List<UserModel>();
                var settled = active ? 0 : 1;
                await using var connection = new SqlConnection(AppSettings.DatabaseConnectionString);
                await connection.OpenAsync();
                var command = new SqlCommand(
                    $"SELECT ea.[UserId] FROM [dbo].[WoW.EventAssignment] AS ea INNER JOIN [WoW.Events].[dbo].[WoW.Event] AS e ON ea.[EventId] = e.[Id] WHERE ea.[EventId] = @eventId AND e.[Settled] = {settled}",
                    connection);
                command.Parameters.AddWithValue("@eventId", Guid.Parse(eventId));
                command.CommandType = CommandType.Text;
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var protectedUserId = await protectValueMessageSender.SendAsync(new ProtectValueRequestModel
                    {
                        ApiKey = AppSettings.EncryptionMicroServiceApiKey!,
                        Text = reader.GetGuid(0).ToString()
                    });

                    var user = await eventUserEventService.SendAsync(new UserRequestModel 
                    { 
                        ApiKey = AppSettings.UserMicroServiceApiKey!,
                        IdOrEmailAddress = protectedUserId, 
                        IncludePassword = false 
                    }); 
                    userModels.Add(user);
                }

                await reader.CloseAsync();
                await connection.CloseAsync();
                return await Task.FromResult(userModels);

            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
        }
    }
}



