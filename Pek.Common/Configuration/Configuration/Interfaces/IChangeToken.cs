namespace Pek.Configuration.Configuration.Interfaces
{
    public interface IChangeToken
    {
        bool HasChanged { get; }
        bool ActiveChangeCallbacks { get; }
        IDisposable RegisterChangeCallback(Action<object> callback, object state);
    }
}