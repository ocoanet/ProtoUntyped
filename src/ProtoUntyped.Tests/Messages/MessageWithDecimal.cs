using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithDecimal : IHasRequiredDecodeOptions
{
    [ProtoMember(1)]
    public decimal Value { get; set; }

    public ProtoDecodeOptions GetRequiredDecodeOptions() => new() { DecodeDecimal = true };
}