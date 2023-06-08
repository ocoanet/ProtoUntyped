using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithMultipleNestedTypes
{
    [ProtoMember(1)]
    public int Id { get; set; }

    [ProtoMember(2)]
    public NestedType1 Nested1 { get; set; }

    [ProtoContract]
    public class NestedType1
    {
        [ProtoMember(1)]
        public NestedType2 Nested2 { get; set; }
    }
        
    [ProtoContract]
    public class NestedType2
    {
        [ProtoMember(1)]
        public int Value { get; set; }
    }
        
}