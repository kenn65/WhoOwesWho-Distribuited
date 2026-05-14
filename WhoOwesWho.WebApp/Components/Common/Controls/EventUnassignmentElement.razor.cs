using Microsoft.AspNetCore.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Protection;

namespace WhoOwesWho.WebApp.Components.Common.Controls;

public partial class EventUnassignmentElement(ICookiesMasterService cookiesMasterService, IProtectionUseCase protectionUseCase)
{
    [Parameter] public bool IsProcessing { get; set; }
    [Parameter] public EventCallback<EventUnassignmentRequestModel> HandleUnassign { get; set; }
    [Parameter] public EventResponseModel? EventResponseModel { get; set; }

    [SupplyParameterFromForm(FormName = "eventunassignment")]
    private EventUnassignmentRequestModel? EventUnassignmentRequestModel { get; set; }
    private CookiesResponseModel? cookies;
    private string userId = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        cookies = await cookiesMasterService.GetAsync();
        userId = await protectionUseCase.ExecuteUnprotectAsync(cookies!.UserIdValue);

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
