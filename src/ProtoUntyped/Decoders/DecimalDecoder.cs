using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProtoBuf;

namespace ProtoUntyped.Decoders;

internal static class DecimalDecoder
{
    public static ProtoWireObject EncodeDecimal(decimal value)
    {
        var dec = new DecimalAccessor(value);
        var a = (ulong)dec.Mid << 32;
        var b = (ulong)dec.Lo & 0xFFFFFFFFL;
        var low = a | b;
        var high = (uint)dec.Hi;
        var signScale = (uint)(((dec.Flags >> 15) & 0x01FE) | ((dec.Flags >> 31) & 0x0001));

        var fields = new List<ProtoWireField>();
        
        if (low != default)
            fields.Add(new ProtoWireField(1, (long)low, WireType.Varint));
        
        if (high != default)
            fields.Add(new ProtoWireField(2, (int)high, WireType.Varint));
        
        if (signScale != default)
            fields.Add(new ProtoWireField(3, (int)signScale, WireType.Varint));

        return new ProtoWireObject(fields);
    }

    public static decimal? TryParseDecimal(IReadOnlyList<ProtoWireField> fields, ProtoDecodeOptions options)
    {
        if (fields.Count is 0 or > 3)
            return null;

        var loField = 0ul;
        var highField = 0u;
        var signScaleField = 0u;

        foreach (var member in fields)
        {
            if (member.Value.Type != ProtoWireValueType.Int64)
                return null;

            var value = member.Value.Int64Value;

            switch (member)
            {
                case { FieldNumber: 1, WireType: WireType.Varint }:
                    loField = (ulong)value;
                    break;

                case { FieldNumber: 2, WireType: WireType.Varint } when value <= uint.MaxValue:
                    highField = (uint)value;
                    break;

                case { FieldNumber: 3, WireType: WireType.Varint } when value <= uint.MaxValue:
                    signScaleField = (uint)value;
                    break;

                default:
                    return null;
            }
        }

        var lo = (int)(loField & 0xFFFFFFFFL);
        var mid = (int)((loField >> 32) & 0xFFFFFFFFL);
        var hi = (int)highField;
        var isNegative = (signScaleField & 0x0001) == 0x0001;
        var scale = (byte)((signScaleField & 0x01FE) >> 1);
        if (scale > 28)
            return null;

        var result = new decimal(lo, mid, hi, isNegative, scale);

        return options.DecimalValidator.Invoke(result) ? result : null;
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct DecimalAccessor
    {
        [FieldOffset(0)] public readonly int Flags;
        [FieldOffset(4)] public readonly int Hi;
        [FieldOffset(8)] public readonly int Lo;
        [FieldOffset(12)] public readonly int Mid;

        [FieldOffset(0)] public readonly decimal Decimal;

        public DecimalAccessor(decimal value)
        {
            this = default;
            Decimal = value;
        }
    }
}