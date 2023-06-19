using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithNestedTypes
{
    [ProtoMember(1)]
    public int Id { get; set; }

    [ProtoMember(2)]
    public NestedType1 Nested1 { get; set; }
    
    [ProtoMember(3)]
    public string Key { get; set; }

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
        public int Value1 { get; set; }
        
        [ProtoMember(2)]
        public string Value2 { get; set; }
    }
        
}