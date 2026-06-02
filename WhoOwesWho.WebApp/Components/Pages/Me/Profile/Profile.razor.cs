using Mapster;
using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Components.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Profile;

public partial class Profile(
    NavigationManager nav,
    IAlertService alertService,
    IUserUseCase userUseCase,
    IEventsUseCase eventsUseCase,
    IProtectionUseCase protectionUseCase
    ) : AuthorizationComponentBase
{
    [SupplyParameterFromForm]
    private UserProfileResponseModel? UserProfileResponseModel { get; set; }
    private bool IsProcessing = false;
   
    protected override async Task OnInitializedAsync()
    {
        if (!await EnsureAuthorizedAsync())
        {
            return;
        }
        UserProfileResponseModel = await GetUserAsync();
    }

    private async Task<UserProfileResponseModel> GetUserAsync()
    {
        var response = await userUseCase.ExecuteAsync(CurrentUserId);
        return response.Adapt<UserProfileResponseModel>();
    }

    private async Task HandleSubmit()
    {
        IsProcessing = true;
        var eventUserResponse = await eventsUseCase.ExecuteGetUserAssignmentAsync(CurrentUserId, true);
        var eventId = eventUserResponse!.EventId.ToString();
        
        var requestModel = new UserUpdateRequestModel
        {
            ProtectedId = await protectionUseCase.ExecuteProtectAsync(CurrentUserId.ToString()),
            FullName = UserProfileResponseModel?.FullName!,
            MobilePhoneNumber = UserProfileResponseModel?.MobilePhoneNumber!,
            Admin = UserProfileResponseModel!.Admin,
            EventId = eventId
        };
        var response = await userUseCase.ExecuteAsync(CurrentUserId, requestModel);
        await StopProcessing();
        if (!response.Success)
        {
            await alertService.Error(response.Message!);
            nav.NavigateTo("/me/profile", true);
            return;
        }
        await alertService.Success(response.Message!);
        nav.NavigateTo("/me/profile", true);
    }
    
    private async Task StopProcessing()
    {
        IsProcessing = false;
        StateHasChanged();
        await Task.Yield();
    }
}
