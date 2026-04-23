using Microsoft.AspNetCore.Components;

namespace WhoOwesWho.WebApp.Services
{
    public interface IHostNameService
    {
        Task<string> GetAsync();
    }

    public class HostNameService(NavigationManager nav) : IHostNameService
    {
        public async Task<string> GetAsync()
        {
            var uri = new Uri(nav.Uri);
            return $"{uri.Host}:{uri.Port}";
        }
    }
}
