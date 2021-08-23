using System.IO;
using ProtoBuf;

namespace ProtoUntyped.Tests
{
    public static class ProtoBufUtil
    {
        public static byte[] Serialize<T>(T obj)
        {
            using var stream = new MemoryStream();

            Serializer.Serialize(stream, obj);

            return stream.ToArray();
        }
    }
}
