using ProtoBuf;

namespace ProtoUntyped.Tests.Messages
{
    [ProtoContract]
    public class MessageWithArrays
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public int[] Int32Array { get; set; }
        
        [ProtoMember(3)]
        public Nested[] MessageArray { get; set; }

        [ProtoContract]
        public class Nested
        {
            [ProtoMember(1)]
            public int Id { get; set; }

            [ProtoMember(2)]
            public string Key { get; set; }
        }
    }
}
