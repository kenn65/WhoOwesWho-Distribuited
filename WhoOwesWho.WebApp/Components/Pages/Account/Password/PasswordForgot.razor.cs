using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Pages.Account.Password;
public partial class PasswordForgot(
    NavigationManager nav, 
    IUserUseCase userUseCase, 
    IAlertService alertService, 
    IHostNameService hostNameService)
{
    [SupplyParameterFromForm]
    private ForgotPasswordRequestModel? ForgotPasswordRequestModel { get; set; }

    private bool IsProcessing = false;

    protected override async Task OnInitializedAsync()
    {
        ForgotPasswordRequestModel ??= new ForgotPasswordRequestModel();
        ForgotPasswordRequestModel!.Host = await hostNameService.GetAsync();
    }

    private async Task HandleSubmit()
    {
        IsProcessing = true;
        StateHasChanged();
        await Task.Yield();
        var response = await userUseCase.ExecuteAsync(ForgotPasswordRequestModel!);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            nav.NavigateTo("/account/password/forgot", forceLoad: true);
            return;
        }
        await alertService.Success(response.Message!);
        nav.NavigateTo("/", forceLoad: true);
    }

    private async Task StopProcessing()
    {
        IsProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}
