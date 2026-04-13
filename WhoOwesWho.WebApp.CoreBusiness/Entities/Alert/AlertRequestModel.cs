namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Alert
{
    public enum AlertType
    {
        Normal,
        Success,
        Error,
        Warning,
        Info,
        Confirmation
    }

    public class AlertRequestModel
    {
        public string Message { get; set; } = "";
        public AlertType Type { get; set; } = AlertType.Normal;
        public TaskCompletionSource<bool>? Completion { get; set; }
    }
}
