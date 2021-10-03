using ProtoBuf;

namespace ProtoUntyped.Tests.Messages
{
    [ProtoContract]
    public class MessageWithString
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Data { get; set; }
    }
}
