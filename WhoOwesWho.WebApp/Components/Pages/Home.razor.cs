using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Pages;

public partial class Home(
    NavigationManager nav,
    ICurrentUserService currentUserService,
    IAuthorizationUseCase authorizationUseCase)
{
    protected override async Task OnInitializedAsync()
    {
        var userId = await currentUserService.GetUserIdAsync();
        if (userId == Guid.Empty)
        {
            await authorizationUseCase.ExecuteRefreshTokenAsync();
        }
        if (await currentUserService.GetIsAuthorizedAsync())
        {
            nav.NavigateTo("/me");
        }
    }
}
