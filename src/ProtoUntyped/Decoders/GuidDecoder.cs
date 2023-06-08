using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ProtoBuf;

namespace ProtoUntyped.Decoders;

internal static class GuidDecoder
{
    public static Guid? TryParseGuid(ProtoObject protoObject, ProtoDecodeOptions options)
    {
        if (protoObject.Members.Count is not 2)
            return null;

        if (protoObject.Members[0] is not ProtoField field1 || field1.FieldNumber != 1 || field1.WireType != WireType.Fixed64)
            return null;

        if (protoObject.Members[1] is not ProtoField field2 || field2.FieldNumber != 2 || field2.WireType != WireType.Fixed64)
            return null;

        var lowAsDouble = (double)field1.Value;
        var low = Unsafe.As<double, ulong>(ref lowAsDouble);

        var highAsDouble = (double)field2.Value;
        var high = Unsafe.As<double, ulong>(ref highAsDouble);

        var guidAccessor = new GuidAccessor(low, high);
        var isValid = options.GuidValidator.Invoke((guidAccessor.Guid, guidAccessor.Version));
            
        return isValid ? guidAccessor.Guid : null;
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
    }
}