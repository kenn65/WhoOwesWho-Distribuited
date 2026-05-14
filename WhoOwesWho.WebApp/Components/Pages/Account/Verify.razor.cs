using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Account;
public partial class Verify(
    NavigationManager nav, 
    IUserUseCase userUseCase, 
    IAlertService alertService,
    IProtectionUseCase protectionUseCase)
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "emailAddress")]
    public string EmailAddress { get; set; } = string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await VerifyEmailAddressAsync();
        }
    }

    private async Task VerifyEmailAddressAsync()
    {
        var requestModel = new VerificationRequestModel
        {
            EmailAddress = await protectionUseCase.ExecuteUnprotectAsync(EmailAddress)
        };
        var response = await userUseCase.ExecuteAsync(requestModel);
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            nav.NavigateTo("/", forceLoad: true);
            return;
        }
        else
        {
            await alertService.Success(response.Message!);
            nav.NavigateTo("/account/signin", forceLoad: true);
        }
    }
}
