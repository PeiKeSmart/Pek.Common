namespace Pek.Configuration.Serialization
{
    public interface IConfigurationSerializer
    {
        string Serialize<T>(T configuration);
        T Deserialize<T>(string data);
    }
}