using System.Collections.Generic;
using System.Diagnostics;
using ProtoBuf;

namespace ProtoUntyped;

[DebuggerDisplay("FieldNumber = {FieldNumber}, WireType = {WireType}, Value = {" + nameof(ProtoUntypedDebuggerDisplay) + "." + nameof(ProtoUntypedDebuggerDisplay.GetDebugValue) + "(this)}")]
public class ProtoField
{
    public ProtoField(int fieldNumber, object value, WireType wireType)
    {
        FieldNumber = fieldNumber;
        Value = value;
        WireType = wireType;
    }

    public int FieldNumber { get; }
    public object Value { get; }
    public WireType WireType { get; }

    public bool IsGroup => WireType == WireType.StartGroup;

    public virtual IEnumerable<ProtoValue> GetProtoValues()
    {
        yield return new ProtoValue(Value, WireType);
    }

    public override string ToString()
    {
        return ProtoFormatter.Default.BuildString(this);
    }
}