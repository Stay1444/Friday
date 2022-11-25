using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Friday.Common;

public static class FridayYaml
{
    private static ISerializer? _serializer;
    private static IDeserializer? _deserializer;

    public static ISerializer Serializer
    {
        get
        {
            if (_serializer is not null) return _serializer;
            _serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            return _serializer;
        }
    }

    public static IDeserializer Deserializer
    {
        get
        {
            if (_deserializer is not null) return _deserializer;
            _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            return _deserializer;
        }
    }
}