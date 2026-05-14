using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.StateHandlers;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Events;

public partial class Events(
    NavigationManager nav,
    IEventsUseCase eventUseCase,
    IAlertService alertService,
    ICookiesMasterService cookiesMasterService,
    IStateHandler<EventModel> eventState,
    IProtectionUseCase protectionUseCase)
{
    private bool IsAdmin { get; set; }
    private Guid UserId { get; set; }
    private IEnumerable<EventResponseModel>? EventList { get; set; }
    private CookiesResponseModel? Cookies = null;

    protected override async Task OnInitializedAsync()
    {
        Cookies = await cookiesMasterService.GetAsync();
        UserId = Guid.Parse(await protectionUseCase.ExecuteUnprotectAsync(Cookies!.UserIdValue));
        IsAdmin = await cookiesMasterService.IsAdministratorAsync(Cookies);
        if (IsAdmin)
        {
            EventList = await GetAdminEventsAsync();
        }
        else
        {
            EventList = await GetUserEventsAsync();
        }
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
            var response = await eventUseCase.ExecuteDeleteEventAsync(eventModel.Id, Cookies!.TokenValue);
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
       return (await eventUseCase.ExecuteGetEventsAsync(UserId, Cookies!.TokenValue))?.Data!;
    }
    private async Task<IEnumerable<EventResponseModel>> GetUserEventsAsync()
    {
        return (await eventUseCase.ExecuteGetEventsAsync(true, Cookies!.TokenValue))?.Data!;
    }

    
}
