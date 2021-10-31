using ProtoBuf;

namespace ProtoUntyped.Decoders
{
    internal static class DecimalDecoder
    {
        public static decimal? TryParseDecimal(ProtoObject protoObject, ProtoDecodeOptions options)
        {
            if (protoObject.Members.Count is 0 or > 3)
                return null;

            var loField = 0ul;
            var highField = 0u;
            var signScaleField = 0u;

            foreach (var member in protoObject.Members)
            {
                if (member.Value is not long value)
                    return null;
                
                switch (member)
                {
                    case ProtoField { FieldNumber: 1, WireType: WireType.Varint }:
                        loField = (ulong)value;
                        break;
                    
                    case ProtoField { FieldNumber: 2, WireType: WireType.Varint } when value <= uint.MaxValue:
                        highField = (uint)value;
                        break;
                    
                    case ProtoField { FieldNumber: 3, WireType: WireType.Varint } when value <= uint.MaxValue:
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
}
