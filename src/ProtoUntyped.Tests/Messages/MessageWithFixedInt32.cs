using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithFixedInt32
{
    [ProtoMember(1)]
    public int Id { get; set; }
    
    [ProtoMember(2, DataFormat = DataFormat.FixedSize)]
    public int FixedValue { get; set; }
    
    [ProtoMember(3)]
    public int Value { get; set; }

    [ProtoMember(4)]
    public double DoubleValue { get; set; }
}