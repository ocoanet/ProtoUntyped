using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithPackedVarintArray
{
    [ProtoMember(1)]
    public int Id { get; set; }
    
    [ProtoMember(2, IsPacked = true)]
    public int[] Int32Array { get; set; }
    
    [ProtoMember(3, IsPacked = true)]
    public long[] Int64Array { get; set; }
}