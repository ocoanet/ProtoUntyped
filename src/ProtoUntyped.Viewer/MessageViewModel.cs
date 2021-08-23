using System.IO;
using System.Text.Json;
using ProtoBuf;

namespace ProtoUntyped.Viewer
{
    public class MessageViewModel
    {
        public MessageViewModel(object message)
        {
            TypeName = message.GetType().FullName;
            JsonText = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true });
            
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, message);
            ProtoBytes = stream.ToArray();
        }

        public string TypeName { get; set; }
        public string JsonText { get; set; }
        public byte[] ProtoBytes { get; set; }
    }
}
