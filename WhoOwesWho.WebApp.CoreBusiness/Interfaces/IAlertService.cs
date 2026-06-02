using WhoOwesWho.WebApp.CoreBusiness.Entities.Alert;

namespace WhoOwesWho.WebApp.CoreBusiness.Interfaces
{
    public interface IAlertService
    {
        event Func<AlertRequestModel, Task>? OnShow;
        Task Show(AlertRequestModel request);
        Task Success(string message);
        Task Error(string message);
        Task Info(string message);
        Task<bool> Confirm(string message);
    }
}
