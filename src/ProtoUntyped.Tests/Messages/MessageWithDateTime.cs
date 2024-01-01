using System;
using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithDateTime : IHasRequiredDecodeOptions
{
    [ProtoMember(1)]
    public DateTime Timestamp { get; set; }

    public ProtoDecodeOptions GetRequiredDecodeOptions() => new() { DecodeDateTime = true };
}