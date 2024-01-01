using System;
using ProtoBuf;

namespace ProtoUntyped;

partial class ProtoField
{
    public ProtoField(int fieldNumber, string value)
        : this(fieldNumber, WireType.String, value)
    {
    }
    
    public ProtoField(int fieldNumber, byte[] value)
        : this(fieldNumber, WireType.String, value)
    {
    }
    
    public ProtoField(int fieldNumber, ProtoObject value)
        : this(fieldNumber, WireType.String, value)
    {
    }
    
    public ProtoField(int fieldNumber, int value, WireType wireType = WireType.Varint)
        : this(fieldNumber, wireType, (object)value)
    {
    }
    
    public ProtoField(int fieldNumber, long value, WireType wireType = WireType.Varint)
        : this(fieldNumber, wireType, (object)value)
    {
    }
    
    public ProtoField(int fieldNumber, float value)
        : this(fieldNumber, WireType.Fixed32, (object)value)
    {
    }
    
    public ProtoField(int fieldNumber, double value)
        : this(fieldNumber, WireType.Fixed64, (object)value)
    {
    }
    
    public ProtoField(int fieldNumber, int[] value)
        : this(fieldNumber, WireType.String, (object)value, WireType.Varint)
    {
    }
    
    public ProtoField(int fieldNumber, long[] value)
        : this(fieldNumber, WireType.String, (object)value, WireType.Varint)
    {
    }
    
    public ProtoField(int fieldNumber, decimal value)
        : this(fieldNumber, WireType.String, (object)value)
    {
    }
    
    public ProtoField(int fieldNumber, DateTime value)
        : this(fieldNumber, WireType.String, (object)value)
    {
    }
    
    public ProtoField(int fieldNumber, TimeSpan value)
        : this(fieldNumber, WireType.String, (object)value)
    {
    }
    
    public ProtoField(int fieldNumber, Guid value)
        : this(fieldNumber, WireType.String, (object)value)
    {
    }
}