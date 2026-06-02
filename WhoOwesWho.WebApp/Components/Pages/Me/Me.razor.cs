using WhoOwesWho.WebApp.Components.Base;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Pages.Me;

public partial class Me(IUserUseCase userUseCase) : AuthorizationComponentBase
{
    private string userName = string.Empty;
    
    protected override async Task OnInitializedAsync()
    {
        if (!await EnsureAuthorizedAsync())
        {
            return;
        }
        var response = await userUseCase.ExecuteAsync(CurrentUserId, false);
        userName = $"Hi {response!.FullName!}";
    }
}

