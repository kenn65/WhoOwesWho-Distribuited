using WhoOwesWho.WebApp.CoreBusiness.Entities.Alert;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;

namespace WhoOwesWho.WebApp.Services
{
    public class AlertService : IAlertService
    {
        public event Func<AlertRequestModel, Task>? OnShow;

        public async Task Show(AlertRequestModel request)
        {
            if (OnShow != null)
                await OnShow.Invoke(request);
        }

        public Task Success(string message) =>
            Show(new AlertRequestModel { Message = message, Type = AlertType.Success });

        public Task Error(string message) =>
            Show(new AlertRequestModel { Message = message, Type = AlertType.Error });

        public Task Info(string message) =>
            Show(new AlertRequestModel { Message = message, Type = AlertType.Info });

        public async Task<bool> Confirm(string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            await Show(new AlertRequestModel
            {
                Message = message,
                Type = AlertType.Confirmation,
                Completion = tcs
            });

            return await tcs.Task;
        }
    }
}
