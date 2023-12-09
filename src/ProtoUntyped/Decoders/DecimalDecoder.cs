using System.Collections.Generic;
using ProtoBuf;

namespace ProtoUntyped.Decoders;

internal static class DecimalDecoder
{
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
}