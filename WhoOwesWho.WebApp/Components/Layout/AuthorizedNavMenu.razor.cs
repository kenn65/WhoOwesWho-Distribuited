using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WhoOwesWho.WebApp.Services;

namespace WhoOwesWho.WebApp.Components.Layout;
public partial class AuthorizedNavMenu(NavigationManager nav, ICookiesMasterService cookiesMasterService)
{
    private async Task HandleSignOut(MouseEventArgs args)
    {
        await cookiesMasterService.DeleteCookiesAsync();
        nav.NavigateTo("/", forceLoad: true);
    }
}
