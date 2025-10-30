namespace Common.Interfaces
{
    using System.IO;

    public interface IJson
    {
        string Serialize(object value);
        string Serialize(Stream stream);
        T Desrialize<T>(string value);
    }
}
