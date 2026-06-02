using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Base
{
    public abstract class AuthorizationComponentBase : ComponentBase
    {
        [Inject]
        protected ICurrentUserService CurrentUserService { get; set; } = null!;

        [Inject]
        protected IAuthorizationUseCase AuthorizationUseCase { get; set; } = null!;
    
        protected Guid CurrentUserId { get; private set; }

        protected async Task<bool> EnsureAuthorizedAsync()
        {
            CurrentUserId = await CurrentUserService.GetUserIdAsync();

            if (CurrentUserId != Guid.Empty)
            {
                return true;
            }

            var response = await AuthorizationUseCase.ExecuteRefreshTokenAsync();

            if (!response.Success)
            {
                return false;
            }

            CurrentUserId = await CurrentUserService.GetUserIdAsync();
            return CurrentUserId != Guid.Empty;
        }
    }
}
