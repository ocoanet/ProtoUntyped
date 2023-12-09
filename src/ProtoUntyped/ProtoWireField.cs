using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace ProtoUntyped;

/// <summary>
/// Represents a raw unconverted field read from the wire format.
/// </summary>
[DebuggerDisplay("FieldNumber = {FieldNumber}, WireType = {WireType}, Value = {Value}")]
public class ProtoWireField
{
    public ProtoWireField(int fieldNumber, ProtoWireValue value, WireType wireType)
    {
        FieldNumber = fieldNumber;
        WireType = wireType;
        Value = value;
    }

    public ProtoWireField(int fieldNumber, int value, WireType wireType = WireType.Varint)
    {
        FieldNumber = fieldNumber;
        Value = new ProtoWireValue(value);
        WireType = wireType;
    }
    
    public ProtoWireField(int fieldNumber, long value, WireType wireType = WireType.Varint)
    {
        FieldNumber = fieldNumber;
        Value = new ProtoWireValue(value);
        WireType = wireType;
    }
    
    public ProtoWireField(int fieldNumber, string value)
    {
        FieldNumber = fieldNumber;
        Value = new ProtoWireValue(value);
        WireType = WireType.String;
    }
    
    public ProtoWireField(int fieldNumber, float value)
    {
        FieldNumber = fieldNumber;
        WireType = WireType.Fixed32;
        Value = new ProtoWireValue(Unsafe.As<float, int>(ref value));
    }
    
    public ProtoWireField(int fieldNumber, double value)
    {
        FieldNumber = fieldNumber;
        WireType = WireType.Fixed64;
        Value = new ProtoWireValue(Unsafe.As<double, long>(ref value));
    }
    
    public ProtoWireField(int fieldNumber, byte[] value)
    {
        FieldNumber = fieldNumber;
        WireType = WireType.String;
        Value = new ProtoWireValue(value);
    }
    
    public ProtoWireField(int fieldNumber, ProtoWireObject value)
        : this(fieldNumber, WireType.String, value)
    {
    }
    
    public ProtoWireField(int fieldNumber, WireType wireType, ProtoWireObject value)
    {
        FieldNumber = fieldNumber;
        WireType = wireType;
        Value = new ProtoWireValue(value);
    }
    
    public int FieldNumber { get; }
    public WireType WireType { get; }
    public ProtoWireValue Value { get; }
}