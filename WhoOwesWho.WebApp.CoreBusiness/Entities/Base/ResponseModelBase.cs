namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Base
{
    public abstract class ResponseModelBase
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }

    }
}
