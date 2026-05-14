using Mapster;
using Microsoft.EntityFrameworkCore;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.EfCore.Context;
using WhoOwesWho.UserService.Models;

namespace WhoOwesWho.UserService.Repositories
{
    public interface IUserQueryRepository
    {
        Task<UserModel?> GetSingleUserByEmailAddressAsync(string? emailAddress, bool complete = false);
        Task<UserModel?> GetSingleUserByIdAsync(Guid id, bool complete = false);
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        Task<ForgotPasswordTokenModel> GetForgotPasswordTokenAsync(Guid userId);
        Task<bool> GetUserEmailExists(string emailAddress);
        Task<bool> GetUserFullNameExists(string fullName);
    }

    public class UserQueryRepository(UserDbContext context) : IUserQueryRepository
    {
        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            return await context.Users.ProjectToType<UserModel>().ToListAsync();
        }

        public async Task<ForgotPasswordTokenModel> GetForgotPasswordTokenAsync(Guid userId)
        {
            var model = await context.ForgotPasswords.Where(x => x.UserId == userId)
                .ProjectToType<ForgotPasswordTokenModel>().FirstOrDefaultAsync();
            return model!;
        }

        public async Task<UserModel?> GetSingleUserByEmailAddressAsync(string? emailAddress, bool complete = false)
        {
            var model = await context.Users.Where(x => x.EmailAddress == emailAddress)
                .ProjectToType<UserModel>().FirstOrDefaultAsync();
            if (!complete && model != null)
            {
                model!.Password = null;
            }
            return model;
        }

        public async Task<UserModel?> GetSingleUserByIdAsync(Guid id, bool complete = false)
        {
            var model = await context.Users.Where(x => x.Id == id)
                .ProjectToType<UserModel>().FirstOrDefaultAsync();
            if (!complete && model != null)
            {
                model!.Password = null;
            }
            return model;
        }

        public async Task<bool> GetUserEmailExists(string emailAddress)
        {
            var output = await context.Users.Where(x => x.EmailAddress == emailAddress).FirstOrDefaultAsync();
            if (output is null)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> GetUserFullNameExists(string fullName)
        {
            var output = await context.Users.Where(x => x.FullName == fullName).FirstOrDefaultAsync();
            if (output is null)
            {
                return false;
            }
            return true;
        }
    }
}
