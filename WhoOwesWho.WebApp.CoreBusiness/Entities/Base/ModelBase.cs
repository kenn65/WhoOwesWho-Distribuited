namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Base
{
    public abstract class ModelBase
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; }
        public string? ExceptionMessage { get; set; }
    }
}
