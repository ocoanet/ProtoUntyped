using System;
using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithGuid
{
    [ProtoMember(1)]
    public Guid Guid { get; set; }
}