using System;
using System.IO;
using System.Text.Json;
using System.Windows.Documents;
using ProtoBuf;

namespace ProtoUntyped.Viewer
{
    public class MessageViewModel
    {
        public MessageViewModel(object message)
        {
            TypeName = message.GetType().FullName;
            MessageJson = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true });
            
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, message);
            ProtoObject = ProtoObject.Decode(stream.GetBuffer().AsMemory(0, (int)stream.Length), new ProtoDecodeOptions { DecodeGuids = true });
        }

        public string TypeName { get; set; }
        public string MessageJson { get; set; }
        public ProtoObject ProtoObject { get; set; }
        public string ProtoObjectJson => JsonSerializer.Serialize(ProtoObject.ToFieldDictionary(), new JsonSerializerOptions { WriteIndented = true });
        public string ProtoObjectString => ProtoObject.ToString();
    }
}
