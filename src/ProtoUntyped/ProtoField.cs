using System.Collections.Generic;
using System.Diagnostics;
using ProtoBuf;

namespace ProtoUntyped;

[DebuggerDisplay("FieldNumber = {FieldNumber}, WireType = {WireType}, Value = {" + nameof(ProtoUntypedDebuggerDisplay) + "." + nameof(ProtoUntypedDebuggerDisplay.GetDebugValue) + "(this)}")]
public class ProtoField
{
    public ProtoField(int fieldNumber, WireType wireType, object value, WireType packedWireType = WireType.None)
    {
        FieldNumber = fieldNumber;
        Value = value;
        WireType = wireType;
        PackedWireType = packedWireType;
    }

    public int FieldNumber { get; }
    public object Value { get; }
    public WireType WireType { get; }
    public WireType PackedWireType { get; }

    public bool IsGroup => WireType == WireType.StartGroup;

    public virtual IEnumerable<object> GetValues()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return ProtoFormatter.Default.BuildString(this);
    }
}