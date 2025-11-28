using Microsoft.Data.SqlClient;
using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IDataQueryService
    {
        Task<UserModel?> GetSingleUserByEmailAddressAsync(string? emailAddress, bool complete = false);
        Task<UserModel?> GetSingleUserByIdAsync(Guid id, bool complete = false);
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        Task<ForgotPasswordTokenModel> GetForgotPasswordTokenAsync(Guid userId);
    }

    public class DataQueryService(IConfiguration configuration) : ServiceBase(configuration), IDataQueryService
    {
        public async Task<UserModel?> GetSingleUserByEmailAddressAsync(string? emailAddress, bool complete = false)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                throw new ArgumentException("Email address argument was not provided.");
            }

            UserModel? entity = null;
            try
            {
                using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "SELECT " +
                        "[Id], " +
                        "[FullName], " +
                        "[EmailAddress], " +
                        "[MobilePhoneNumber], " +
                        "[Admin], " +
                        "[Password], " +
                        "[EmailAddressVerified], " +
                        "[Balance] FROM [WoW.Users].[dbo].[WoW.User] " +
                        "WHERE [EmailAddress] = @emailAddress",
                        connection);
                    command.Parameters.AddWithValue("@emailAddress", emailAddress);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        entity = new UserModel
                        {
                            Id = reader.GetGuid(0),
                            FullName = reader.GetString(1),
                            EmailAddress = reader.GetString(2),
                            MobilePhoneNumber = reader.GetString(3),
                            Admin = reader.GetBoolean(4),
                            Password = complete ? reader.GetString(5) : string.Empty,
                            EmailAddressVerified = reader.GetBoolean(6),
                            Balance = reader.GetDecimal(7)
                        };
                    }

                    connection.Close();
                }

                return entity;
            }
            catch (SqlException e)
            {
                throw new Exception(
                    $"An error occurred while retrieving the user with email address {emailAddress}: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"An error occurred while retrieving the user with email address {emailAddress}: {e.Message}", e);
            }
        }

        public async Task<UserModel?> GetSingleUserByIdAsync(Guid id, bool complete = false)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Email address argument was not provided.");
            }

            UserModel? entity = null;
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "SELECT " +
                        "[Id], " +
                        "[FullName], " +
                        "[EmailAddress], " +
                        "[MobilePhoneNumber], " +
                        "[Admin], " +
                        "[Password], " +
                        "[EmailAddressVerified], " +
                        "[Balance] FROM [WoW.Users].[dbo].[WoW.User] " +
                        "WHERE [Id] = @id",
                        connection);
                    command.Parameters.AddWithValue("@id", id);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        entity = new UserModel
                        {
                            Id = reader.GetGuid(0),
                            FullName = reader.GetString(1),
                            EmailAddress = reader.GetString(2),
                            MobilePhoneNumber = reader.GetString(3),
                            Admin = reader.GetBoolean(4),
                            Password = complete ? reader.GetString(5) : string.Empty,
                            EmailAddressVerified = reader.GetBoolean(6),
                            Balance = reader.GetDecimal(7)
                        };
                    }

                    connection.Close();
                }

                return await Task.FromResult(entity);
            }
            catch (SqlException e)
            {
                throw new Exception($"An error occurred while retrieving the user with Id {id}: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while retrieving the user with Id {id}: {e.Message}", e);
            }
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            var entities = new List<UserModel>();
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "SELECT " +
                        "[Id], " +
                        "[FullName], " +
                        "[EmailAddress], " +
                        "[MobilePhoneNumber], " +
                        "[Admin], " +
                        "[Password], " +
                        "[EmailAddressVerified], " +
                        "[Balance] FROM [WoW.Users].[dbo].[WoW.User]", connection);
                    command.CommandType = System.Data.CommandType.Text;
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        entities.Add(
                            new UserModel
                            {
                                Id = reader.GetGuid(0),
                                FullName = reader.GetString(1),
                                EmailAddress = reader.GetString(2),
                                MobilePhoneNumber = reader.GetString(3),
                                Admin = reader.GetBoolean(4),
                                Password = reader.GetString(5),
                                EmailAddressVerified = reader.GetBoolean(6),
                                Balance = reader.GetDecimal(7)
                            });

                    }

                    connection.Close();
                }

                return await Task.FromResult(entities);
            }
            catch (SqlException e)
            {
                throw new Exception($"An error occurred while retrieving all users: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while retrieving all users: {e.Message}", e);
            }
        }

        public async Task<ForgotPasswordTokenModel> GetForgotPasswordTokenAsync(Guid userId)
        {
            try
            {
                var response = new ForgotPasswordTokenModel();
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "SELECT [UserId], [ForgotPasswordToken], [ForgotPasswordTimeStamp] FROM [WoW.Users].[dbo].[WoW.ForgotPassword] WHERE [UserId] = @userId",
                        connection);
                    command.Parameters.AddWithValue("@userId", userId.ToString());
                    command.CommandType = System.Data.CommandType.Text;
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        response.UserId = reader.GetGuid(0);
                        response.ForgotPasswordToken = reader.GetString(1);
                        response.ExpirationTime = reader.GetInt64(2);
                        break;
                    }

                    await reader.CloseAsync();
                    await connection.CloseAsync();
                }

                return await Task.FromResult(response);
            }
            catch (SqlException e)
            {
                throw new Exception(
                    $"An error occurred while retrieving the forgot password token for user {userId}: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"An error occurred while retrieving the forgot password token for user {userId}: {e.Message}", e);
            }
        }
    }
}


