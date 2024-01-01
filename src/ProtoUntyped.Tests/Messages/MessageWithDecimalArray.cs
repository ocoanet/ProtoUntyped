using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithDecimalArray : IHasRequiredDecodeOptions
{
    [ProtoMember(1)]
    public decimal[] Values { get; set; }

    public ProtoDecodeOptions GetRequiredDecodeOptions() => new() { DecodeDecimal = true };
}
