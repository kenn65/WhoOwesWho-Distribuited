namespace WhoOwesWho.Shared.Models.Base
{
    public class EnumerableWrapperResponseModel<T> : ModelBase
    {
        public T? Data { get; set; }
    }
}
