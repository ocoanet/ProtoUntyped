using System;
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
}
