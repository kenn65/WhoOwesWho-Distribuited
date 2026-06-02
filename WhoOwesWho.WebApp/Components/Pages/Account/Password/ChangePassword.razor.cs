using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Components.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Pages.Account.Password;
public partial class ChangePassword(
    NavigationManager nav, 
    IUserUseCase userUseCase, 
    IAlertService alertService,
    IAuthorizationUseCase authorizationUseCase
    ) : AuthorizationComponentBase
{
    [SupplyParameterFromForm]
    private ChangePasswordRequestModel? ChangePasswordRequestModel { get; set; }

    private bool IsProcessing = false;

    protected override async Task OnInitializedAsync()
    {
        ChangePasswordRequestModel ??= new ChangePasswordRequestModel();
    }

    private async Task HandleSubmit()
    {
        if (!await EnsureAuthorizedAsync())
        {
            return;
        }
        IsProcessing = true;
        ChangePasswordRequestModel!.EmailAddress = await CurrentUserService.GetEmailAddressAsync() ;
        var response = await userUseCase.ExecuteAsync(ChangePasswordRequestModel);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            nav.NavigateTo("/me/profile/password", true);
            return;
        }
        await authorizationUseCase.ExecuteDeleteCookiesAsync();
        await alertService.Success(response.Message!);
        nav.NavigateTo("/account/signin", true);
    }

    private async Task StopProcessing()
    {
        IsProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}
