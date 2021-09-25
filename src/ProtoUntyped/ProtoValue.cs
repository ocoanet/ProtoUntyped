using ProtoBuf;

// TODO: replace by struct

namespace ProtoUntyped
{
    public class ProtoValue
    {
        public ProtoValue(object value, WireType wireType)
        {
            Value = value;
            WireType = wireType;
        }

        public object Value { get; }
        public WireType WireType { get; }
    }
}
