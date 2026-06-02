namespace WhoOwesWho.WebApp.CoreBusiness.Interfaces
{
    public interface IAuthorizationCoordinator
    {
        Task<bool> RefreshAsync();
    }
}
