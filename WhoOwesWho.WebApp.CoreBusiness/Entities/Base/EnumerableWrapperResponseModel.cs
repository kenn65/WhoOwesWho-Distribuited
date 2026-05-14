namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Base
{
    public sealed class EnumerableWrapperResponseModel<T> : ResponseModelBase
    {
        public T? Data { get; set; }
    }
}
