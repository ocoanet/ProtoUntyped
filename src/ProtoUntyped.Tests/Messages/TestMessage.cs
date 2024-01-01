using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class TestMessage : IHasRequiredDecodeOptions
{
    [ProtoMember(1)] public int Id { get; set; }

    [ProtoMember(2)] public string Name { get; set; }

    [ProtoMember(3)] public decimal Amount { get; set; }

    [ProtoMember(4)] public TestNestedMessage NestedMessage { get; set; }
    
    public ProtoDecodeOptions GetRequiredDecodeOptions() => new() { DecodeDecimal = true };
}