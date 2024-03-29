using System;
using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithGuid : IHasRequiredDecodeOptions
{
    [ProtoMember(1)]
    public Guid Guid { get; set; }

    public ProtoDecodeOptions GetRequiredDecodeOptions() => new() { DecodeGuid = true };
}