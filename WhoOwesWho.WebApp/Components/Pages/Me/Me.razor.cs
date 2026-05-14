using Microsoft.AspNetCore.Components;
using System.Net;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Me;
public partial class Me(
    NavigationManager nav, 
    ICookiesMasterService cookiesMasterService, 
    IUserUseCase userUseCase,
    IProtectionUseCase protectionUseCase)
{
    private string _userName = string.Empty;
    
    protected override async Task OnInitializedAsync()
    {
        var cookies = await cookiesMasterService.GetAsync();
        var userId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(cookies!.UserIdValue));
        if (cookies is null)
        {
            nav.NavigateTo("/", true);
        }
        else
        {
            var response = await userUseCase.ExecuteAsync(
                userId,
                cookies.TokenValue,
                false);
            _userName = $"Hi {response!.FullName!}";
        }
    }
}
