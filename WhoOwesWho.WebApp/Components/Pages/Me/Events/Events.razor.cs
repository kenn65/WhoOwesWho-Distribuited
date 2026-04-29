using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.StateHandlers;
using WhoOwesWho.WebApp.UseCases.Events;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Events;
public partial class Events(
    NavigationManager nav,
    IEventsUseCase eventUseCase,
    IAlertService alertService,
    ICookiesMasterService cookiesMasterService,
    IStateHandler<EventModel> eventState)
{
    private bool IsAdmin { get; set; }
    private IEnumerable<EventResponseModel>? EventList { get; set; }
    private CookiesResponseModel? Cookies = null;

    protected override async Task OnInitializedAsync()
    {
        Cookies = await cookiesMasterService.GetAsync();
        IsAdmin = await cookiesMasterService.IsAdministratorAsync();
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
            var response = await eventUseCase.ExecuteAsync(eventModel.Id, Cookies!.TokenValue);
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
        return await eventUseCase.ExecuteAsync(Cookies!.UserIdValue, Cookies.TokenValue, false);
    }
        private async Task<IEnumerable<EventResponseModel>> GetUserEventsAsync()
    {
        return await eventUseCase.ExecuteAsync(true, Cookies!.TokenValue);
    }
}
