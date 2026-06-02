using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Pages.Account;
public partial class SignIn(NavigationManager nav, IAuthorizationUseCase authorizationUseCase, IAlertService alertService, IHostNameService hostNameService)
{
    [Parameter] public string Code { get; set; } = string.Empty;

    [SupplyParameterFromForm]
    private AuthenticationRequestModel? AuthenticationRequestModel { get; set; }

    [SupplyParameterFromForm]
    public AuthenticationCodeRequestModel? AuthenticationCodeRequestModel { get; set; }

    private AuthenticationResponseModel AutenticationResponseModel { get; set; } = new();
    
    private bool IsProcessing = false;

    protected override async Task OnInitializedAsync()
    {
        AuthenticationRequestModel ??= new AuthenticationRequestModel();
        AuthenticationCodeRequestModel ??= new AuthenticationCodeRequestModel();
        AuthenticationRequestModel!.Host = await hostNameService.GetAsync();
    }

    private async Task HandleAuthenticationAsync(EditContext args)
    {
        IsProcessing = true;
        StateHasChanged();
        await Task.Yield();

        AutenticationResponseModel = await authorizationUseCase.ExecuteAuthenticateAsync(AuthenticationRequestModel!);
        await StopProcessing();
        if (!AutenticationResponseModel.Success)
        {
            await alertService.Error(AutenticationResponseModel.Message!);
        }
        else
        {
            Code = AutenticationResponseModel.Code;
            await alertService.Success(AutenticationResponseModel.Message!);
        }
    }

    private async Task HandleAuthorizationAsync(EditContext args)
    {
        IsProcessing = true;
        StateHasChanged();
        await Task.Yield();

        if (Code != AuthenticationCodeRequestModel!.Code)
        {
            await StopProcessing();
            await alertService.Error("Invalid authentication code");
            nav.NavigateTo("/account/signin", forceLoad: true);
            return;
        }

        var requestModel = new AuthorizationRequestModel
        {
            EmailAddress = AuthenticationRequestModel!.EmailAddress,
        };

        var response = await authorizationUseCase.ExecuteAuthorizeAsync(requestModel);
        if (!response.Success)
        {
            await StopProcessing();
            await alertService.Error("An error occurred while signing in. Did you verify your e-mail address?");
            nav.NavigateTo("/account/signin", forceLoad: true);
            return;
        }
        nav.NavigateTo("/me", forceLoad: true);
    }

    private void GotoSignUp()
    {
        nav.NavigateTo("/account/signup", forceLoad: true);
    }

    private async Task StopProcessing()
    {
        IsProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}
