using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Common.Controls;

public partial class EventUnassignmentElement(
    ICurrentUserService currentUserService)
{
    [Parameter] public bool IsProcessing { get; set; }
    [Parameter] public EventCallback<EventUnassignmentRequestModel> HandleUnassign { get; set; }
    [Parameter] public EventResponseModel? EventResponseModel { get; set; }

    [SupplyParameterFromForm(FormName = "eventunassignment")]
    private EventUnassignmentRequestModel? EventUnassignmentRequestModel { get; set; }
    private string userId = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        userId = (await currentUserService.GetUserIdAsync()).ToString();

        EventUnassignmentRequestModel = new EventUnassignmentRequestModel
        {
            EventIdString = EventResponseModel!.Id.ToString()
        };
    }

    private async Task HandleSubmit()
    {
        IsProcessing = true;
        if (EventUnassignmentRequestModel != null)
        {
            EventUnassignmentRequestModel.UserIdString = userId;
            await HandleUnassign.InvokeAsync(EventUnassignmentRequestModel);
        }
    }
}
