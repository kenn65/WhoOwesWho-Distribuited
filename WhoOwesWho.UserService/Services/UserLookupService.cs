using WhoOwesWho.Shared.Models;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services.Base;

namespace WhoOwesWho.UserService.Services
{
    public interface IUserLookupService
    {
        Task<UserModel?> GetSingleUserByEmailAddressAsync(string? emailAddress, bool complete = false);
        Task<UserModel?> GetSingleUserByIdAsync(Guid id, bool complete = false);
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
    }

    public class UserLookupService(IConfiguration configuration, IUserQueryRepository userQueryRepository) : ServiceBase(configuration), IUserLookupService
    {
        public async Task<UserModel?> GetSingleUserByEmailAddressAsync(string? emailAddress, bool complete = false)
        {
            var response = await userQueryRepository.GetSingleUserByEmailAddressAsync(emailAddress, complete);
            response?.Success = response != null;
            return response;
        }

        public async Task<UserModel?> GetSingleUserByIdAsync(Guid id, bool complete = false)
        {
            var user = await userQueryRepository.GetSingleUserByIdAsync(id, complete);
            user?.Success = user != null;
            return user;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            return await userQueryRepository.GetAllUsersAsync();
        }
    }
}
