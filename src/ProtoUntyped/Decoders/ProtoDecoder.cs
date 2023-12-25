using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace ProtoUntyped.Decoders;

internal static class ProtoDecoder
{
    public static bool HasValidFieldHeader(Span<byte> bytes)
    {
        if (!TryReadFieldHeader(bytes, out var tag))
            return false;

        var fieldNumber = (int)(tag >> 3);
        if (fieldNumber < 1)
            return false;

        var wireType = (WireType)(tag & 7);
        if (wireType == WireType.EndGroup || wireType > WireType.Fixed32)
            return false;

        return true;
    }

    private static bool TryReadFieldHeader(Span<byte> bytes, out uint value)
    {
        if (bytes.Length == 0)
        {
            value = default;
            return false;
        }

        value = bytes[0];
        if ((value & 0x80) == 0)
            return true;

        value &= 0x7F;
        if (bytes.Length == 1)
            return false;

        uint chunk = bytes[1];
        value |= (chunk & 0x7F) << 7;
        if ((chunk & 0x80) == 0)
            return true;

        if (bytes.Length == 2)
            return false;

        chunk = bytes[2];
        value |= (chunk & 0x7F) << 14;
        if ((chunk & 0x80) == 0)
            return true;

        if (bytes.Length == 3)
            return false;

        chunk = bytes[3];
        value |= (chunk & 0x7F) << 21;
        if ((chunk & 0x80) == 0)
            return true;

        if (bytes.Length == 4)
            return false;

        chunk = bytes[4];
        value |= chunk << 28;

        return (chunk & 0xF0) == 0;
    }

#if NETSTANDARD2_1
    public static bool TryDecode(ReadOnlyMemory<byte> data, ProtoDecodeOptions options, [NotNullWhen(true)] out ProtoObject? protoObject)
#else
    public static bool TryDecode(ReadOnlyMemory<byte> data, ProtoDecodeOptions options, out ProtoObject? protoObject)
#endif
    {
        if (ProtoWireDecoder.TryDecode(data, options, out var wireObject))
        {
            protoObject = Decode(wireObject!, options);
            return true;
        }

        protoObject = default;
        return false;
    }

    public static ProtoObject Decode(ProtoWireObject wireObject, ProtoDecodeOptions decodeOptions)
    {
        return new ProtoObject(ConvertWireFields(wireObject.Fields, decodeOptions));
    }

    private static List<ProtoField> ConvertWireFields(IReadOnlyList<ProtoWireField> wireFields, ProtoDecodeOptions decodeOptions)
    {
        var fields = wireFields.Select(x => ConvertWireField(x, decodeOptions)).ToList();
        var groupedFields = GroupRepeatedFields(fields);
        
        return groupedFields;
    }

    private static ProtoField ConvertWireField(ProtoWireField wireField, ProtoDecodeOptions decodeOptions)
    {
        switch (wireField.Value.Type)
        {
            case ProtoWireValueType.String:
                return CreateField(wireField, wireField.Value.StringValue);

            case ProtoWireValueType.Bytes:
                return CreateField(wireField, wireField.Value.BytesValue);

            case ProtoWireValueType.Message:
                return CreateField(wireField, DecodeMessageValue(wireField, decodeOptions));

            case ProtoWireValueType.Int32:
                return CreateField(wireField, DecodeInt32Value(wireField, decodeOptions));

            case ProtoWireValueType.Int64:
                return CreateField(wireField, DecodeInt64Value(wireField, decodeOptions));
            
            case ProtoWireValueType.Int32Array:
                return CreateField(wireField, wireField.Value.Int32Value);
            
            case ProtoWireValueType.Int64Array:
                return CreateField(wireField, wireField.Value.Int64Value);

            default:
                throw new NotSupportedException($"Unknown value type {wireField.Value.Type}");
        }
    }

    private static ProtoField CreateField(ProtoWireField wireField, object value)
    {
        return new ProtoField(wireField.FieldNumber, wireField.WireType, value, wireField.PackedWireType);
    }

    private static List<ProtoField> GroupRepeatedFields(IReadOnlyList<ProtoField> fields)
    {
        return fields.GroupBy(x => x.FieldNumber)
                     .Select(g => g.Count() == 1 ? g.Single() : new RepeatedProtoField(g.Key, g.ToArray()))
                     .ToList();
    }
    
    private static object DecodeMessageValue(ProtoWireField wireField, ProtoDecodeOptions decodeOptions)
    {
        var messageFields = wireField.Value.MessageValue.Fields;
        
        if (decodeOptions.DecodeGuid && GuidDecoder.TryParseGuid(messageFields, decodeOptions) is { } guid)
            return guid;

        if (decodeOptions.DecodeDateTime && TimeDecoder.TryParseDateTime(messageFields, decodeOptions) is { } dateTime)
            return dateTime;

        if (decodeOptions.DecodeTimeSpan && TimeDecoder.TryParseTimeSpan(messageFields, decodeOptions) is { } timeSpan)
            return timeSpan;

        if (decodeOptions.DecodeDecimal && DecimalDecoder.TryParseDecimal(messageFields, decodeOptions) is { } dec)
            return dec;

        return new ProtoObject(ConvertWireFields(messageFields, decodeOptions));
    }
    
    private static object DecodeInt32Value(ProtoWireField wireField, ProtoDecodeOptions decodeOptions)
    {
        var value = wireField.Value.Int32Value;

        if (wireField.WireType == WireType.Fixed32 && decodeOptions.Fixed32DecodingMode == FixedWireTypeDecodingMode.FloatingPoint)
            return Unsafe.As<int, float>(ref value);

        return value;
    }
    
    private static object DecodeInt64Value(ProtoWireField wireField, ProtoDecodeOptions decodeOptions)
    {
        var value = wireField.Value.Int64Value;

        if (wireField.WireType == WireType.Fixed64 && decodeOptions.Fixed64DecodingMode == FixedWireTypeDecodingMode.FloatingPoint)
            return Unsafe.As<long, double>(ref value);

        return value;
    }
}
