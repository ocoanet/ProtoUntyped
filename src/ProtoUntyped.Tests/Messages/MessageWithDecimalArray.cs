using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithDecimalArray
{
    [ProtoMember(1)]
    public decimal[] Values { get; set; }
}
