using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.Components.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.StateHandlers;
using WhoOwesWho.WebApp.UseCases.Events;

namespace WhoOwesWho.WebApp.Components.Pages.Me.Events;

public partial class Events(
    NavigationManager nav,
    IEventsUseCase eventsUseCase,
    IAlertService alertService,
    IStateHandler<EventModel> eventState
    ) : AuthorizationComponentBase
{
    private bool isAdministrator;
    private IEnumerable<EventResponseModel>? eventList; 
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        if (!await EnsureAuthorizedAsync())
        {
            return;
        }
        isAdministrator = await CurrentUserService.GetIsAdminAsync();
        eventList = isAdministrator
                ? await GetAdminEventsAsync()
                : await GetUserEventsAsync();
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
            var response = await eventsUseCase.ExecuteDeleteEventAsync(eventModel.Id);
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
        return await eventsUseCase.ExecuteGetEventsAsync(CurrentUserId);
    }

    private async Task<IEnumerable<EventResponseModel>> GetUserEventsAsync()
    {
        return await eventsUseCase.ExecuteGetEventsAsync(true);
    }


}
