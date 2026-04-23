using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Services;

namespace WhoOwesWho.WebApp.Components.Pages;
public partial class Home(
    NavigationManager nav,
    ICookiesMasterService cookiesMasterService)
{
    protected override async Task OnInitializedAsync()
    {
        if (await cookiesMasterService.IsAuthorizedAsync())
        {
            nav.NavigateTo("/me");
        }
    }
}
