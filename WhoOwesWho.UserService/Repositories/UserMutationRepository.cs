using Mapster;
using Microsoft.EntityFrameworkCore;
using WhoOwesWho.Models.Models;
using WhoOwesWho.UserService.EfCore.Context;
using WhoOwesWho.UserService.EfCore.DataModels;
using WhoOwesWho.UserService.Models;


namespace WhoOwesWho.UserService.Repositories
{
    public interface IUserMutationRepository
    {
        Task<UserModel?> CreateUserAsync(UserModel entity);
        Task<UserModel?> UpdateUserAsync(UserModel entity);
        Task<bool> CreateForgotPasswordTokenAsync(ForgotPasswordTokenModel model);
        Task<bool> DeleteForgotPasswordTokenAsync(Guid userId);
    }

    public class UserMutationRepository(UserDbContext context) : IUserMutationRepository
    {
        async Task<bool> IUserMutationRepository.CreateForgotPasswordTokenAsync(ForgotPasswordTokenModel model)
        {
            try
            {
                var entity = model.Adapt<ForgotPassword>();
                await context.AddAsync(entity);
                await context.SaveChangesAsync();
                return await Task.FromResult(true);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while creating the forgot password token: {e.Message}");
                return await Task.FromResult(false);
            }
        }

        public async Task<UserModel?> CreateUserAsync(UserModel entity)
        {
            try
            {
                var userEntity = entity.Adapt<Users>();
                await context.Users.AddAsync(userEntity);
                await context.SaveChangesAsync();
                return userEntity.Adapt<UserModel>();
            }
            catch (Exception e)
            {
                return new UserModel
                {
                    Message = $"An error occurred while creating the user: {e.Message}"
                };  
            }
        }

        public async Task<bool> DeleteForgotPasswordTokenAsync(Guid userId)
        {
            try
            {
                await context.ForgotPasswords
                    .Where(x => x.UserId == userId)
                    .ExecuteDeleteAsync();

                return await Task.FromResult(true);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while deleting the forgot password token: {e.Message}");
                return false;
            }
        }

        public async Task<UserModel?> UpdateUserAsync(UserModel entity)
        {
            try
            {
                await context.Users
                .Where(u => u.Id == entity.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.FullName, entity.FullName)
                    .SetProperty(u => u.EmailAddress, entity.EmailAddress)
                    .SetProperty(u => u.MobilePhoneNumber, entity.MobilePhoneNumber)
                    .SetProperty(u => u.Admin, entity.Admin)
                    .SetProperty(u => u.Password, entity.Password)
                    .SetProperty(u => u.EmailAddressVerified, entity.EmailAddressVerified));

                var response = await context.Users
                    .Where(u => u.Id == entity.Id)
                    .ProjectToType<UserModel>()
                    .FirstOrDefaultAsync();
                response!.Success = true;
               return response;
            }
            catch 
            {
                return new UserModel();
            }
        }
    }
}
