using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Password;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Pages.Account.Password;
public partial class PasswordReset(NavigationManager nav, IUserUseCase userUseCase, IAlertService alertService)
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "emailAddress")]
    public string EmailAddress { get; set; } = string.Empty;

    [Parameter]
    [SupplyParameterFromQuery(Name = "token")]
    public string Token { get; set; } = string.Empty;

    [SupplyParameterFromForm]
    private ResetPasswordRequestModel? ResetPasswordRequestModel { get; set; }

    private bool IsProcessing = false;

    protected override async Task OnInitializedAsync()
    {
        ResetPasswordRequestModel ??= new ResetPasswordRequestModel();
        await VerifyToken();
    }

    private async Task VerifyToken()
    {
        var verificationResponse = await userUseCase.ExecuteAsync(EmailAddress, Token);
        if (!verificationResponse.Success)
        {
            await alertService.Error(verificationResponse.Message!);
            nav.NavigateTo("/account/signin", forceLoad: true);
        }
    }

    private async Task HandleSubmit()
    {
        IsProcessing = true;
        ResetPasswordRequestModel!.EmailAddress = EmailAddress;
        var response = await userUseCase.ExecuteAsync(ResetPasswordRequestModel!);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            ResetPasswordRequestModel.NewPassword = string.Empty;
            ResetPasswordRequestModel.NewPasswordRepeat = string.Empty;
            //nav.NavigateTo("/account/password/reset");
            return;
        }
        await alertService.Success(response.Message!);
        nav.NavigateTo("/account/signin", forceLoad: true);
    }

    private async Task StopProcessing()
    {
        IsProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}
