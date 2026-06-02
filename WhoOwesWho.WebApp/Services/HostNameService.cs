using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;

namespace WhoOwesWho.WebApp.Services
{
    public class HostNameService(NavigationManager nav) : IHostNameService
    {
        public async Task<string> GetAsync()
        {
            var uri = new Uri(nav.Uri);
            return $"{uri.Host}:{uri.Port}";
        }
    }
}
