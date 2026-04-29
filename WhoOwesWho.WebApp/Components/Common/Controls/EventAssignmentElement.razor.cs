using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;

namespace WhoOwesWho.WebApp.Components.Common.Controls;
public partial class EventAssignmentElement
{
    [Parameter] public bool IsProcessing { get; set; }
    [Parameter] public IEnumerable<EventResponseModel>? EventList { get; set; }
    [Parameter] public EventCallback<EventAssignmentRequestModel> HandleAssign { get; set; }

    [SupplyParameterFromForm]
    private EventAssignmentRequestModel? EventAssignmentRequestModel { get; set; }

    protected override async Task OnInitializedAsync()
    {
        EventAssignmentRequestModel = new EventAssignmentRequestModel();
    }

    private async Task HandleSubmit()
    {
        IsProcessing = true;
        if (EventAssignmentRequestModel != null)
        {
            await HandleAssign.InvokeAsync(EventAssignmentRequestModel);
        }
    }
}
