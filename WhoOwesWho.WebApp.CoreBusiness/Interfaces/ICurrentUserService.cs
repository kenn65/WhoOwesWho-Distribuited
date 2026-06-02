namespace WhoOwesWho.WebApp.CoreBusiness.Interfaces
{
    public interface ICurrentUserService
    {
        Task<Guid> GetUserIdAsync();

        Task<string> GetEmailAddressAsync();

        Task<bool> GetIsAdminAsync();

        Task<bool> GetIsAuthorizedAsync();
    }
}
