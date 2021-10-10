using System;
using ProtoBuf;

namespace ProtoUntyped.Tests.Messages
{
    [ProtoContract]
    public class MessageWithDateTime
    {
        [ProtoMember(1)]
        public DateTime Timestamp { get; set; }
    }
}
