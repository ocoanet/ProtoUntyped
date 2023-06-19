using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithGroups
{
    [ProtoMember(1)]
    public int Id { get; set; }

    [ProtoMember(2, DataFormat = DataFormat.Group)]
    public NestedType1 Nested1 { get; set; }
    
    [ProtoMember(3)]
    public string Key { get; set; }

    [ProtoContract]
    public class NestedType1
    {
        [ProtoMember(1, DataFormat = DataFormat.Group)]
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