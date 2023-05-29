using ProtoBuf;

namespace ProtoUntyped.Tests.Messages
{
    [ProtoContract]
    public class MessageWithByteArray
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public byte[] Data { get; set; }
    }
}
