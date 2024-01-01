using System.Collections.Generic;
using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class TestNestedMessage : IHasRequiredDecodeOptions
{
    [ProtoMember(1)] public string Key { get; set; }

    [ProtoMember(2)] public List<decimal> Values { get; set; }
    
    public ProtoDecodeOptions GetRequiredDecodeOptions() => new() { DecodeDecimal = true };
}