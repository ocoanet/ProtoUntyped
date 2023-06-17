using ProtoBuf;

namespace ProtoUntyped;

public readonly struct ProtoValue
{
    public ProtoValue(object value, WireType wireType)
    {
        Value = value;
        WireType = wireType;
    }

    public object Value { get; }
    public WireType WireType { get; }
}