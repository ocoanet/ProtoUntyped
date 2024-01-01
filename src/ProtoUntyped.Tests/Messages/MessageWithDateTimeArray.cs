using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithDateTimeArray : IHasRequiredDecodeOptions
{
    [ProtoMember(1)]
    public int Id { get; set; }

    [ProtoMember(2)]
    public List<DateTime> Timestamps { get; set; }
    
    public ProtoDecodeOptions GetRequiredDecodeOptions()
    {
        return new ProtoDecodeOptions { DecodeDateTime = true };
    }
}