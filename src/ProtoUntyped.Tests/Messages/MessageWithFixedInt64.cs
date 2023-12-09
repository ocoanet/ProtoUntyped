using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithFixedInt64
{
    [ProtoMember(1)]
    public int Id { get; set; }
    
    [ProtoMember(2, DataFormat = DataFormat.FixedSize)]
    public long FixedValue { get; set; }
    
    [ProtoMember(3)]
    public long Value { get; set; }

    [ProtoMember(4)]
    public float SingleValue { get; set; }
}