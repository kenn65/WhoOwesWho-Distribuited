namespace WhoOwesWho.WebApp.CoreBusiness.Interfaces
{
    public interface IHostNameService
    {
        Task<string> GetAsync();
    }
}
