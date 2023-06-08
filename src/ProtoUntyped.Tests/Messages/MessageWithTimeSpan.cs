using System;
using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithTimeSpan
{
    [ProtoMember(1)]
    public TimeSpan Duration { get; set; }
}