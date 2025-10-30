namespace Common.Handlers
{
    using Common.Interfaces;
    using Newtonsoft.Json;
    using System.IO;

    public class JsonHandler : IJson
    {
        public T Desrialize<T>(string value)
        {
            return (T)JsonConvert.DeserializeObject<T>(value);
        }

        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public string Serialize(Stream stream)
        {
            if(stream == null)
            {
                return string.Empty;
            }
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                return Serialize(serializer.Deserialize(jsonTextReader));
            }
        }
    }
}
