using ProtoBuf;

namespace ProtoUntyped.Tests.Messages;

[ProtoContract]
public class MessageWithIntegers
{
    [ProtoMember(1)]
    public short Int16Value { get; set; }
        
    [ProtoMember(2)]
    public ushort UInt16Value { get; set; }
        
    [ProtoMember(3)]
    public int Int32Value { get; set; }

    [ProtoMember(4)]
    public uint UInt32Value { get; set; }
        
    [ProtoMember(5)]
    public long Int64Value { get; set; }
        
    [ProtoMember(6)]
    public ulong UInt64Value { get; set; }
        
    [ProtoMember(7)]
    public byte ByteValue { get; set; }
        
    [ProtoMember(8)]
    public sbyte SByteValue { get; set; }
}