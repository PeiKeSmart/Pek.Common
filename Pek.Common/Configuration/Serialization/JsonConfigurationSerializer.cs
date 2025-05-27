using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pek.Configuration.Serialization
{
    public class JsonConfigurationSerializer : IConfigurationSerializer
    {
        public async Task<T> DeserializeAsync<T>(string path)
        {
            if (!File.Exists(path))
                return default;

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                return await JsonSerializer.DeserializeAsync<T>(stream);
        }

        public async Task SerializeAsync<T>(string path, T data)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                await JsonSerializer.SerializeAsync(stream, data, options);
        }
    }
}