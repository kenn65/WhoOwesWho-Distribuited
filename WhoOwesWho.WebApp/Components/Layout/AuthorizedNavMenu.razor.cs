using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Layout;
public partial class AuthorizedNavMenu(NavigationManager nav, IAuthorizationUseCase authorizationUseCase)
{
    private bool signOut;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (signOut)
        {
            await authorizationUseCase.ExecuteDeleteCookiesAsync();
            nav.NavigateTo("/", forceLoad: true);
        }
    }

    private async Task HandleSignOut(MouseEventArgs args)
    {
        signOut = true;
    }
}
