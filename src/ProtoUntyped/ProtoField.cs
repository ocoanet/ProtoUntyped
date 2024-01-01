using System;
using System.Diagnostics;
using ProtoBuf;

namespace ProtoUntyped;

[DebuggerDisplay("FieldNumber = {FieldNumber}, WireType = {WireType}, Value = {" + nameof(ProtoUntypedDebuggerDisplay) + "." + nameof(ProtoUntypedDebuggerDisplay.GetDebugValue) + "(this)}")]
public class ProtoField
{
    public ProtoField(int fieldNumber, WireType wireType, string value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, byte[] value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, ProtoObject value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, int value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, long value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, float value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, double value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, int[] value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, long[] value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, decimal value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, DateTime value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, TimeSpan value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    public ProtoField(int fieldNumber, WireType wireType, Guid value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    protected ProtoField(int fieldNumber, WireType wireType, Array value, WireType packedWireType = WireType.None)
        : this(fieldNumber, wireType, (object)value, packedWireType)
    {
    }
    
    private ProtoField(int fieldNumber, WireType wireType, object value, WireType packedWireType = WireType.None)
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

    public override string ToString()
    {
        return ProtoFormatter.Default.BuildString(this);
    }
}