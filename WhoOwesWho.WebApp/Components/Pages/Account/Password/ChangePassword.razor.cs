using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Pages.Account.Password;
public partial class ChangePassword(
    NavigationManager nav, 
    IUserUseCase userUseCase, 
    IAlertService alertService, 
    ICookiesMasterService cookiesMasterService)
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
        IsProcessing = true;
        var cookies = await cookiesMasterService.GetAsync();
        ChangePasswordRequestModel!.EmailAddress = cookies!.UserEmailAddressValue;
        var response = await userUseCase.ExecuteAsync(cookies.TokenValue, ChangePasswordRequestModel!);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            nav.NavigateTo("/me/profile/password", true);
            return;
        }
        await cookiesMasterService.DeleteCookiesAsync();
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
