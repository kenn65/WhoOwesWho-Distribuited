namespace WhoOwesWho.WebApp.StateHandlers
{
    public interface IStateHandler<T> where T : class
    {
        T? SelectedItem { get; set; }
    }

    public class StateHandler<T> : IStateHandler<T> where T : class
    {
        public T? SelectedItem { get; set; }
    }
}
