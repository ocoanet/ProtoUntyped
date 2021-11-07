using System.IO;
using System.Text.Json;
using PropertyChanged;
using ProtoBuf;

namespace ProtoUntyped.Viewer
{
    [AddINotifyPropertyChangedInterface]
    public class MessageViewModel
    {
        public MessageViewModel(object message)
        {
            TypeName = message.GetType().FullName;
            MessageJson = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true });
            
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, message);
            ProtoBytes = stream.ToArray();
        }

        public string TypeName { get; set; }
        public string MessageJson { get; set; }
        public byte[] ProtoBytes { get; set; }
    }
}
