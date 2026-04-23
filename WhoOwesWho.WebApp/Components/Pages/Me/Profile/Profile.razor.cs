using Mapster;
using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Events;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Profile;

public partial class Profile(
    NavigationManager nav,
    IAlertService alertService,
    ICookiesMasterService cookiesMasterService,
    IUserUseCase userUseCase,
    IEventsUseCase eventsUseCase)
{
    [SupplyParameterFromForm]
    private UserProfileResponseModel? UserProfileResponseModel { get; set; }

    private bool IsProcessing = false;
    private CookiesResponseModel? Cookies = null;

    protected override async Task OnInitializedAsync()
    {
        Cookies = await cookiesMasterService.GetAsync();
        UserProfileResponseModel = await GetUserAsync();
    }

    private async Task<UserProfileResponseModel> GetUserAsync()
    {
        var response = await userUseCase.ExecuteAsync(Cookies!.UserIdValue, Cookies.TokenValue, false);
        return response.Adapt<UserProfileResponseModel>();
    }

    private async Task HandleSubmit()
    {
        IsProcessing = true;
        var eventUserResponse = await eventsUseCase.ExecuteAsync(Cookies!.UserIdValue, Cookies.TokenValue);
        var eventId = eventUserResponse!.EventId.ToString();
        var requestModel = new UserUpdateRequestModel
        {
            ProtectedId = Cookies.UserIdValue,
            FullName = UserProfileResponseModel?.FullName!,
            MobilePhoneNumber = UserProfileResponseModel?.MobilePhoneNumber!,
            Admin = UserProfileResponseModel!.Admin,
            EventId = eventId
        };
        var response = await userUseCase.ExecuteAsync(Cookies.UserIdValue, Cookies.TokenValue, requestModel);
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
        if (await cookiesMasterService.IsAdministratorAsync() && !UserProfileResponseModel!.Admin)
        {
            await cookiesMasterService.SetAdminCookieAsync(Cookies!, false);
        }
        if (!await cookiesMasterService.IsAdministratorAsync() && UserProfileResponseModel!.Admin)
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
