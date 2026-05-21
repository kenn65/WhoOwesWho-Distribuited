using Microsoft.AspNetCore.Components;
using System.Net;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.StateHandlers;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Events;

public partial class Events(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    IAlertService alertService,
    ICookiesMasterService cookiesMasterService,
    IStateHandler<EventModel> eventState,
    IProtectionUseCase protectionUseCase)
{
    private bool isAdministrator;
    private Guid userId; 
    private IEnumerable<EventResponseModel>? eventList { get; set; }
    private CookiesResponseModel? cookies = null;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        cookies = await cookiesMasterService.GetAsync();
        userId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(cookies!.UserIdValue));
        isAdministrator = await cookiesMasterService.IsAdministratorAsync(cookies);
        if (isAdministrator)
        {
            eventList = await GetAdminEventsAsync();
        }
        else
        {
            eventList = await GetUserEventsAsync();
        }
        isLoading = false;
    }

    private async Task EditEventAsync(EventModel eventModel)
    {
        eventState.SelectedItem = eventModel;
        nav.NavigateTo("/me/events/edit");
    }

    private async Task DeleteEventAsync(EventModel eventModel)
    {
        var confirmation = await alertService.Confirm("Are you sure you want to delete this event?");
        if (confirmation)
        {
            var response = await eventsUseCase.ExecuteDeleteEventAsync(eventModel.Id, cookies!.TokenValue);
            if (!response.Success)
            {
                await alertService.Error(response.Message!);
                return;
            }
            await alertService.Success(response.Message!);
            nav.NavigateTo("/me/events", true);
        }
    }
    
    private async Task<IEnumerable<EventResponseModel>> GetAdminEventsAsync()
    {
       return await eventsUseCase.ExecuteGetEventsAsync(userId, cookies!.TokenValue);
    }

    private async Task<IEnumerable<EventResponseModel>> GetUserEventsAsync()
    {
        return await eventsUseCase.ExecuteGetEventsAsync(true, cookies!.TokenValue);
    }

    
}
