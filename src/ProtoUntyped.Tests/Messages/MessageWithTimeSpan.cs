using System;
using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithTimeSpan : IHasRequiredDecodeOptions
{
    [ProtoMember(1)]
    public TimeSpan Duration { get; set; }

    public ProtoDecodeOptions GetRequiredDecodeOptions() => new() { DecodeTimeSpan = true };
}