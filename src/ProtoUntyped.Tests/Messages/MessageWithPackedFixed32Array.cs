using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithPackedFixed32Array
{
    [ProtoMember(1)]
    public int Id { get; set; }
    
    [ProtoMember(2, IsPacked = true, DataFormat = DataFormat.FixedSize)]
    public int[] Int32Array { get; set; }
}