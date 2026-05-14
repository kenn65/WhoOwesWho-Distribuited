using Mapster;
using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Profile;

public partial class Profile(
    NavigationManager nav,
    IAlertService alertService,
    ICookiesMasterService cookiesMasterService,
    IUserUseCase userUseCase,
    IEventsUseCase eventsUseCase,
    IProtectionUseCase protectionUseCase)
{
    [SupplyParameterFromForm]
    private UserProfileResponseModel? UserProfileResponseModel { get; set; }

    private bool IsProcessing = false;
    private CookiesResponseModel? Cookies = null;
    private Guid UserId = Guid.Empty; 

    protected override async Task OnInitializedAsync()
    {
        Cookies = await cookiesMasterService.GetAsync();
        UserId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(Cookies!.UserIdValue));
        UserProfileResponseModel = await GetUserAsync();
    }

    private async Task<UserProfileResponseModel> GetUserAsync()
    {
        var response = await userUseCase.ExecuteAsync(UserId, Cookies!.TokenValue);
        return response.Adapt<UserProfileResponseModel>();
    }

    private async Task HandleSubmit()
    {
        IsProcessing = true;
        var eventUserResponse = await eventsUseCase.ExecuteGetUserAssignmentAsync(UserId, Cookies!.TokenValue);
        var eventId = eventUserResponse!.EventId.ToString();
        var requestModel = new UserUpdateRequestModel
        {
            ProtectedId = Cookies.UserIdValue,
            FullName = UserProfileResponseModel?.FullName!,
            MobilePhoneNumber = UserProfileResponseModel?.MobilePhoneNumber!,
            Admin = UserProfileResponseModel!.Admin,
            EventId = eventId
        };
        var response = await userUseCase.ExecuteAsync(UserId, Cookies.TokenValue, requestModel);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            nav.NavigateTo("/me/profile", true);
            return;
        }
        await HandleAdminCookieAsync();
        await alertService.Success(response.Message!);
        nav.NavigateTo("/me/profile", true);
    }

    private async Task HandleAdminCookieAsync()
    {
        if (await cookiesMasterService.IsAdministratorAsync(Cookies!) && !UserProfileResponseModel!.Admin)
        {
            await cookiesMasterService.SetAdminCookieAsync(Cookies!, false);
        }
        if (!await cookiesMasterService.IsAdministratorAsync(Cookies!) && UserProfileResponseModel!.Admin)
        {
            await cookiesMasterService.SetAdminCookieAsync(Cookies!, true);
        }
    }

    private async Task StopProcessing()
    {
        IsProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}
