using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProtoUntyped;

[StructLayout(LayoutKind.Explicit)]
[DebuggerTypeProxy(typeof(ProtoWireValueDebugView))]
[DebuggerDisplay("Type = {Type}, Value = {GetValue()}")]
public readonly struct ProtoWireValue
{
    [FieldOffset(0)] private readonly string? _stringValue;
    [FieldOffset(0)] private readonly byte[]? _bytesValue;
    [FieldOffset(0)] private readonly ProtoWireObject? _message;
    [FieldOffset(8)] private readonly int _int32Value;
    [FieldOffset(8)] private readonly long _int64Value;
    [FieldOffset(16)] private readonly ProtoWireValueType _type;
    
    public ProtoWireValue(string value)
    {
        _type = ProtoWireValueType.String;
        _stringValue = value;
    }
    
    public ProtoWireValue(byte[] value)
    {
        _type = ProtoWireValueType.Bytes;
        _bytesValue = value;
    }

    public ProtoWireValue(ProtoWireObject value)
    {
        _type = ProtoWireValueType.Message;
        _message = value;
    }

    public ProtoWireValue(int value)
    {
        _type = ProtoWireValueType.Int32;
        _int32Value = value;
    }
    
    public ProtoWireValue(long value)
    {
        _type = ProtoWireValueType.Int64;
        _int64Value = value;
    }

    public ProtoWireValueType Type => _type;
    
    public string StringValue => _type == ProtoWireValueType.String ? _stringValue! : string.Empty;
    
    public byte[] BytesValue => _type == ProtoWireValueType.Bytes ? _bytesValue! : Array.Empty<byte>();
    
    public ProtoWireObject Message => _type == ProtoWireValueType.Message ? _message! : ProtoWireObject.Empty;

    public int Int32Value => _type == ProtoWireValueType.Int32 ? _int32Value : default;

    public long Int64Value => _type == ProtoWireValueType.Int64 ? _int64Value : default;

    public object GetValue()
    {
        return _type switch
        {
            ProtoWireValueType.Bytes   => BytesValue,
            ProtoWireValueType.Int32   => Int32Value,
            ProtoWireValueType.Int64   => Int64Value,
            ProtoWireValueType.Message => Message,
            ProtoWireValueType.String  => StringValue,
            _                          => throw new NotSupportedException($"Unknown value type {_type}"),
        };
    }

    public override string ToString()
    {
        return GetValue().ToString();
    }
}