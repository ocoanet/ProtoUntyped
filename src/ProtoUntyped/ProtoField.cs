using System.Diagnostics;
using ProtoBuf;

namespace ProtoUntyped;

[DebuggerDisplay("FieldNumber = {FieldNumber}, WireType = {WireType}, Value = {" + nameof(ProtoUntypedDebuggerDisplay) + "." + nameof(ProtoUntypedDebuggerDisplay.GetDebugValue) + "(this)}")]
public class ProtoField : ProtoMember
{
    public ProtoField(int fieldNumber, object value, WireType wireType)
        : base(fieldNumber, value)
    {
        WireType = wireType;
    }

    public ProtoField(int fieldNumber, ProtoObject value)
        : this(fieldNumber, value, WireType.String)
    {
    }

    public WireType WireType { get; }
    
    public override void Accept(ProtoObjectVisitor visitor)
    {
        if (Value is ProtoObject protoObject)
            protoObject.Accept(visitor);
    }

    public override string ToString()
    {
        return ProtoFormatter.Default.BuildString(this);
    }
}