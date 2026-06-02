using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.UseCases.Account;

namespace WhoOwesWho.WebApp.Components.Pages.Account;
public partial class SignUp(NavigationManager nav, IUserUseCase userUseCase, IAlertService alertService, IHostNameService hostNameService)
{
    private bool IsProcessing = false;

    [SupplyParameterFromForm]
    private UserModel? UserModel { get; set; }

    protected override void OnInitialized()
    {
        UserModel ??= new UserModel();
    }        

    private async Task HandleSubmit(EditContext args)
    {
        IsProcessing = true;
        StateHasChanged();
        await Task.Yield();

        var requestModel = new SignUpRequestModel
        {
            Entity = UserModel,
            Host = await hostNameService.GetAsync()
        };
        var response = await userUseCase.ExecuteAsync(requestModel);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            nav.NavigateTo("account/signup", forceLoad: true);
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
