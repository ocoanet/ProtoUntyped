using ProtoBuf;

namespace ProtoUntyped.Tests.Messages
{
    [ProtoContract]
    public class MessageWithNestedType
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public NestedType Nested { get; set; }

        [ProtoMember(3)]
        public string Key { get; set; }

        [ProtoContract]
        public class NestedType
        {
            [ProtoMember(1)]
            public int Value1 { get; set; }

            [ProtoMember(2)]
            public string Value2 { get; set; }
        }
    }
}
