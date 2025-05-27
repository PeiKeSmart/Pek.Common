namespace Pek.Configuration.Configuration.Interfaces
{
    public interface IConfigurationProvider
    {
        T GetValue<T>(string key);
        void SetValue<T>(string key, T value);
        void Load();
        void Save();
        event Action ConfigurationChanged;
    }
}