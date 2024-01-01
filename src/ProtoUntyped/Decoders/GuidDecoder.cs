using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProtoBuf;

namespace ProtoUntyped.Decoders;

internal static class GuidDecoder
{
    public static Guid? TryParseGuid(IReadOnlyList<ProtoWireField> fields, ProtoDecodeOptions options)
    {
        if (fields.Count is not 2)
            return null;

        if (fields[0] is not { } field1 || field1.FieldNumber != 1 || field1.WireType != WireType.Fixed64)
            return null;

        if (fields[1] is not { } field2 || field2.FieldNumber != 2 || field2.WireType != WireType.Fixed64)
            return null;
        
        var low = (ulong)field1.Value.Int64Value;
        var high = (ulong)field2.Value.Int64Value;

        var guidAccessor = new GuidAccessor(low, high);
        var isValid = options.GuidValidator.Invoke((guidAccessor.Guid, guidAccessor.Version));
            
        return isValid ? guidAccessor.Guid : null;
    }

    public static ProtoWireObject EncodeGuid(Guid value)
    {
        var guidAccessor = new GuidAccessor(value);

        return new ProtoWireObject(
            new(1, (long)guidAccessor.Low, WireType.Fixed64),
            new(2, (long)guidAccessor.High, WireType.Fixed64)
        );
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct GuidAccessor
    {
        [FieldOffset(0)]
        public readonly Guid Guid;

        [FieldOffset(0)]
        public readonly ulong Low;

        [FieldOffset(8)]
        public readonly ulong High;
            
        /// <remarks>
        /// Version is stored in bits 4-7 of 8th byte (<a href="See https://www.ietf.org/rfc/rfc4122.txt">UUID RFC</a>).
        /// </remarks>
        [FieldOffset(7)]
        public readonly byte VersionByte;

        public byte Version => (byte)((VersionByte & 0xF0) >> 4);

        public GuidAccessor(ulong low, ulong high)
        {
            Guid = default;
            VersionByte = default;
            Low = low;
            High = high;
        }

        public GuidAccessor(Guid guid)
        {
            Low = default;
            High = default;
            VersionByte = default;
            Guid = guid;
        }
    }
}