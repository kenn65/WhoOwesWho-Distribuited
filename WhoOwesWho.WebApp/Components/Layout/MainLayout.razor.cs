using Microsoft.JSInterop;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Alert;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;

namespace WhoOwesWho.WebApp.Components.Layout;

public partial class MainLayout(IAlertService alertService, IJSRuntime JS, ICurrentUserService currentUserService)
{
    private bool isAuthorized;

    protected override async Task OnInitializedAsync()
    {
        isAuthorized = await currentUserService.GetIsAuthorizedAsync();
        alertService.OnShow += HandleAlert;
    }

    private async Task HandleAlert(AlertRequestModel request)
    {
        var result = await JS.InvokeAsync<SwalResult>(
                "blazorSwal.showAdvanced",
                request.Message,
                request.Type.ToString()
            );

        // Handle confirmation dialogs
        if (request.Completion != null)
        {
            request.Completion.SetResult(result.IsConfirmed);
        }
    }

    public class SwalResult
    {
        public bool IsConfirmed { get; set; }
    }
}
