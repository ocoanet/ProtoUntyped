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

    public static ProtoObject Decode(ProtoWireObject wireObject, ProtoDecodeOptions decodeOptions)
    {
        return new ProtoObject(DecodeFields(wireObject.Fields, decodeOptions));
    }

    private static List<ProtoField> DecodeFields(IReadOnlyList<ProtoWireField> wireFields, ProtoDecodeOptions decodeOptions)
    {
        var fields = wireFields.Select(x => DecodeField(x, decodeOptions)).ToList();
        var groupedFields = GroupRepeatedFields(fields);

        return groupedFields;
    }

    private static ProtoField DecodeField(ProtoWireField wireField, ProtoDecodeOptions decodeOptions)
    {
        switch (wireField.Value.Type)
        {
            case ProtoWireValueType.String:
                return new ProtoField(wireField.FieldNumber, wireField.WireType, wireField.Value.StringValue, wireField.PackedWireType);

            case ProtoWireValueType.Bytes:
                return new ProtoField(wireField.FieldNumber, wireField.WireType, wireField.Value.BytesValue, wireField.PackedWireType);

            case ProtoWireValueType.Message:
                return DecodeMessageField(wireField, decodeOptions);

            case ProtoWireValueType.Int32:
                return DecodeInt32Field(wireField, decodeOptions);

            case ProtoWireValueType.Int64:
                return DecodeInt64Value(wireField, decodeOptions);

            case ProtoWireValueType.Int32Array:
                return new ProtoField(wireField.FieldNumber, wireField.WireType, wireField.Value.Int32ArrayValue, wireField.PackedWireType);

            case ProtoWireValueType.Int64Array:
                return new ProtoField(wireField.FieldNumber, wireField.WireType, wireField.Value.Int64ArrayValue, wireField.PackedWireType);

            default:
                throw new NotSupportedException($"Unknown value type {wireField.Value.Type}");
        }
    }

    private static List<ProtoField> GroupRepeatedFields(IReadOnlyList<ProtoField> fields)
    {
        return fields.GroupBy(x => x.FieldNumber)
                     .Select(g => g.Count() == 1 ? g.Single() : new RepeatedProtoField(g.Key, g.ToArray()))
                     .ToList();
    }

    private static ProtoField DecodeMessageField(ProtoWireField wireField, ProtoDecodeOptions decodeOptions)
    {
        var messageFields = wireField.Value.MessageValue.Fields;

        if (decodeOptions.DecodeGuid && GuidDecoder.TryParseGuid(messageFields, decodeOptions) is { } guid)
            return new ProtoField(wireField.FieldNumber, wireField.WireType, guid, wireField.PackedWireType);

        if (decodeOptions.DecodeDateTime && TimeDecoder.TryParseDateTime(messageFields, decodeOptions) is { } dateTime)
            return new ProtoField(wireField.FieldNumber, wireField.WireType, dateTime, wireField.PackedWireType);

        if (decodeOptions.DecodeTimeSpan && TimeDecoder.TryParseTimeSpan(messageFields, decodeOptions) is { } timeSpan)
            return new ProtoField(wireField.FieldNumber, wireField.WireType, timeSpan, wireField.PackedWireType);

        if (decodeOptions.DecodeDecimal && DecimalDecoder.TryParseDecimal(messageFields, decodeOptions) is { } dec)
            return new ProtoField(wireField.FieldNumber, wireField.WireType, dec, wireField.PackedWireType);

        var protoObject = new ProtoObject(DecodeFields(messageFields, decodeOptions));
        return new ProtoField(wireField.FieldNumber, wireField.WireType, protoObject, wireField.PackedWireType);
    }

    private static ProtoField DecodeInt32Field(ProtoWireField wireField, ProtoDecodeOptions decodeOptions)
    {
        var value = wireField.Value.Int32Value;

        if (wireField.WireType == WireType.Fixed32 && decodeOptions.Fixed32DecodingMode == FixedWireTypeDecodingMode.FloatingPoint)
            return new ProtoField(wireField.FieldNumber, wireField.WireType, Unsafe.As<int, float>(ref value), wireField.PackedWireType);

        return new ProtoField(wireField.FieldNumber, wireField.WireType, value, wireField.PackedWireType);
    }

    private static ProtoField DecodeInt64Value(ProtoWireField wireField, ProtoDecodeOptions decodeOptions)
    {
        var value = wireField.Value.Int64Value;

        if (wireField.WireType == WireType.Fixed64 && decodeOptions.Fixed64DecodingMode == FixedWireTypeDecodingMode.FloatingPoint)
            return new ProtoField(wireField.FieldNumber, wireField.WireType, Unsafe.As<long, double>(ref value), wireField.PackedWireType);

        return new ProtoField(wireField.FieldNumber, wireField.WireType, value, wireField.PackedWireType);
    }
}