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

        [Inject]
        protected IUserUseCase UserUseCase { get; set; } = null!;

        protected Guid CurrentUserId { get; private set; }
        protected bool IsAdmin { get; private set; }

        protected async Task<bool> EnsureAuthorizedAsync()
        {
            CurrentUserId = await CurrentUserService.GetUserIdAsync();
            if (CurrentUserId != Guid.Empty)
            {
                IsAdmin = await UserUseCase.ExecuteAsync(CurrentUserId);
                //IsAdmin = await CurrentUserService.GetIsAdminAsync();
                return true;
            }

            var response = await AuthorizationUseCase.ExecuteRefreshTokenAsync();

            if (!response.Success)
            {
                return false;
            }

            CurrentUserId = await CurrentUserService.GetUserIdAsync();
            IsAdmin = await UserUseCase.ExecuteAsync(CurrentUserId);
            //IsAdmin = await CurrentUserService.GetIsAdminAsync();
            return CurrentUserId != Guid.Empty;
        }
    }
}
