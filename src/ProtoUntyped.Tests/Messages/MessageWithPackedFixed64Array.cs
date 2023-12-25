using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithPackedFixed64Array
{
    [ProtoMember(1)]
    public int Id { get; set; }
    
    [ProtoMember(2, IsPacked = true, DataFormat = DataFormat.FixedSize)]
    public long[] Int64Array { get; set; }
}