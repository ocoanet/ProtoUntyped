using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithDecimal
{
    [ProtoMember(1)]
    public decimal Value { get; set; }
}