using ProtoBuf;

namespace ProtoUntyped.Tests.Messages
{
    [ProtoContract]
    public class MessageWithFloats
    {
        [ProtoMember(1)]
        public double DoubleValue { get; set; }

        [ProtoMember(2)]
        public double SingleValue { get; set; }
    }
}
