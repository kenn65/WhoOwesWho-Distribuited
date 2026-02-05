using Microsoft.Data.SqlClient;
using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.Models;
using WhoOwesWho.UserService.Services.Base;
using WhoOwesWho.UserService.Services.Gateways;
using WhoOwesWho.UserService.Services.ServiceBus.Publishers;

namespace WhoOwesWho.UserService.Services
{
    public interface IDataMutationService
    {
        Task<UserModel?> CreateUserAsync(UserModel entity, string host);
        Task<UserModel?> UpdateUserAsync(UserModel entity);
        Task<bool> CreateForgotPasswordTokenAsync(ForgotPasswordTokenModel model);
        Task<bool> DeleteForgotPasswordTokenAsync(Guid userId);
    }

    public class DataMutationService(
        IConfiguration configuration
        ) : ServiceBase(configuration), IDataMutationService
    {
        public async Task<UserModel?> CreateUserAsync(UserModel entity, string host)
        {
            
            entity.Id = Guid.NewGuid();
            try
            {

                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "INSERT INTO [WoW.Users].[dbo].[WoW.User] (Id, FullName, EmailAddress, MobilePhoneNumber, Admin, Password, EmailAddressVerified, Balance) VALUES (@id, @fullName, @emailAddress, @mobilePhoneNumber, @admin, @password, @emailAddressVerified, @balance)",
                        connection);
                    command.Parameters.AddWithValue("@id", entity.Id);
                    command.Parameters.AddWithValue("@fullName", entity.FullName);
                    command.Parameters.AddWithValue("@emailAddress", entity.EmailAddress);
                    command.Parameters.AddWithValue("@mobilePhoneNumber", entity.MobilePhoneNumber);
                    command.Parameters.AddWithValue("@admin", entity.Admin);
                    command.Parameters.AddWithValue("@password", entity.Password);
                    command.Parameters.AddWithValue("@emailAddressVerified", entity.EmailAddressVerified);
                    command.Parameters.AddWithValue("@balance", entity.Balance);
                    command.CommandType = System.Data.CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }

                return await Task.FromResult(entity);
            }
            catch (SqlException e)
            {
                throw new Exception($"An error occurred while creating the user: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while creating the user: {e.Message}", e);
            }
        }

        public async Task<UserModel?> UpdateUserAsync(UserModel entity)
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "UPDATE [WoW.Users].[dbo].[WoW.User] SET FullName = @fullname, EmailAddress = @emailAddress, MobilePhoneNumber = @mobilePhoneNumber, Admin = @admin, Password = @password, EmailAddressVerified = @emailAddressVerified, Balance = @balance WHERE Id = @id",
                        connection);
                    command.Parameters.AddWithValue("@id", entity.Id);
                    command.Parameters.AddWithValue("@fullName", entity.FullName);
                    command.Parameters.AddWithValue("@emailAddress", entity.EmailAddress);
                    command.Parameters.AddWithValue("@mobilePhoneNumber", entity.MobilePhoneNumber);
                    command.Parameters.AddWithValue("@admin", entity.Admin);
                    command.Parameters.AddWithValue("@password", entity.Password);
                    command.Parameters.AddWithValue("@emailAddressVerified", entity.EmailAddressVerified);
                    command.Parameters.AddWithValue("@balance", entity.Balance);
                    command.CommandType = System.Data.CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }

                entity.Success = true;
                entity.Message = "User was updated successfully.";

            }
            catch (Exception)
            {
                entity.Message = "An unexpected error occurred. Please try again";
            }

            return await Task.FromResult(entity);
        }

        public async Task<bool> CreateForgotPasswordTokenAsync(ForgotPasswordTokenModel model)
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "INSERT INTO [WoW.Users].[dbo].[WoW.ForgotPassword] ([UserId],[ForgotPasswordToken],[ForgotPasswordTimeStamp]) VALUES (@userid, @forgotPasswordToken, @forgotPasswordTimeStamp)",
                        connection);
                    command.Parameters.AddWithValue("@userId", model.UserId);
                    command.Parameters.AddWithValue("@forgotPasswordToken", model.ForgotPasswordToken);
                    command.Parameters.AddWithValue("@forgotPasswordTimeStamp", model.ExpirationTime);
                    command.CommandType = System.Data.CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }

                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> DeleteForgotPasswordTokenAsync(Guid userId)
        {
            try
            {
                await using (var connection = new SqlConnection(AppSettings.DatabaseConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "DELETE FROM [WoW.Users].[dbo].[WoW.ForgotPassword] WHERE [UserId] = @userId",
                        connection);
                    command.Parameters.AddWithValue("@userId", userId.ToString());
                    command.CommandType = System.Data.CommandType.Text;
                    await command.ExecuteNonQueryAsync();
                }

                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }
    }
}