namespace WhoOwesWho.WebApp.CoreBusiness.Interfaces
{
    public interface ICurrentUserService
    {
        Task<Guid> GetUserIdAsync();

        Task<string> GetEmailAddressAsync();

        Task<string> GetUserNameAsync();
        
        Task<bool> GetIsAdminAsync();

        Task<bool> GetIsAuthorizedAsync();
    }
}
