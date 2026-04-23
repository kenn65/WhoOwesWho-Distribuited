using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Pages.Me;
public partial class Me(NavigationManager nav, ICookiesMasterService cookiesMasterService, IUserUseCase userUseCase)
{
    private string _userName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var cookies = await cookiesMasterService.GetAsync();
        if (cookies is null)
        {
            nav.NavigateTo("/", true);
        }
        else
        {
            var response = await userUseCase.ExecuteAsync(
                cookies.UserIdValue,
                cookies.TokenValue,
                false);
            _userName = $"Hi {response!.FullName!}";
        }
    }
}
