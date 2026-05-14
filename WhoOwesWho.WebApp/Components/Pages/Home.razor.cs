using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Services;

namespace WhoOwesWho.WebApp.Components.Pages;
public partial class Home(
    NavigationManager nav,
    ICookiesMasterService cookiesMasterService)
{
    protected override async Task OnInitializedAsync()
    {
        var cookies = await cookiesMasterService.GetAsync();
        if (await cookiesMasterService.IsAuthorizedAsync(cookies!))
        {
            nav.NavigateTo("/me");
        }
    }
}
