using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ProtoBuf;

namespace ProtoUntyped
{
    public class ProtoObject
    {
        public ProtoObject()
            : this(new())
        {
        }

        public ProtoObject(List<ProtoMember> members)
        {
            Members = members;
        }

        public List<ProtoMember> Members { get; }

        public Dictionary<int, object> ToFieldDictionary()
        {
            return Members.ToDictionary(x => x.FieldNumber, x => ConvertValue(x.Value));

            static object ConvertValue(object value)
            {
                return value switch
                {
                    ProtoObject protoObject => protoObject.ToFieldDictionary(),
                    object[] array          => array.Select(x => ConvertValue(x)).ToArray(),
                    _                       => value,
                };
            }
        }

        public override string ToString()
        {
            return ProtoFormatter.BuildString(this);
        }

        public static ProtoObject Decode(ReadOnlyMemory<byte> data)
        {
            return Decode(data, new ProtoDecodeOptions());
        }

        public static ProtoObject Decode(ReadOnlyMemory<byte> data, ProtoDecodeOptions options)
        {
            var fields = GroupRepeatedFields(ReadFields(data, options));

            return new ProtoObject(fields);
        }

        private static List<ProtoMember> GroupRepeatedFields(List<ProtoField> fields)
        {
            return fields.GroupBy(x => x.FieldNumber)
                         .Select(g => g.Count() == 1 ? (ProtoMember)g.Single() : new ProtoArray(g.Key, g.Select(x => new ProtoValue(x.Value, x.WireType)).ToArray()))
                         .ToList();
        }

        private static List<ProtoField> ReadFields(ReadOnlyMemory<byte> data, ProtoDecodeOptions options)
        {
            var fields = new List<ProtoField>();

            var reader = ProtoReader.State.Create(data, null);
            while (ReadField(ref reader, options) is { } field)
            {
                fields.Add(field);
            }

            return fields;
        }

        private static ProtoField? ReadField(ref ProtoReader.State reader, ProtoDecodeOptions options)
        {
            if (reader.ReadFieldHeader() == 0)
                return null;

            var fieldNumber = reader.FieldNumber;
            var wireType = reader.WireType;
            var fieldValue = ReadFieldValue(ref reader, options);

            return new ProtoField(fieldNumber, fieldValue, wireType);
        }

        private static object ReadFieldValue(ref ProtoReader.State reader, ProtoDecodeOptions options)
        {
            switch (reader.WireType)
            {
                case WireType.Varint:
                    return reader.ReadInt64();

                case WireType.Fixed32:
                    // Could be an integer, assume floating point because protobuf-net defaults to varint for integers.
                    return reader.ReadSingle();
                
                case WireType.Fixed64:
                    // Could be an integer, assume floating point because protobuf-net defaults to varint for integers.
                    return reader.ReadDouble();

                case WireType.String:
                    return DecodeString(ref reader, options);

                case WireType.StartGroup:
                case WireType.EndGroup:
                case WireType.SignedVarint:
                    break;
            }

            throw new ArgumentOutOfRangeException();
        }

        private static object DecodeString(ref ProtoReader.State reader, ProtoDecodeOptions options)
        {
            // Can be a string, an embedded message or a byte array.
            
            var bytes = reader.AppendBytes(null);
            
            if (TryReadEmbeddedMessage(bytes, options) is { } embeddedMessage)
                return embeddedMessage;

            return options.StringDecoder.TryDecode(bytes, out var s) ? s : bytes;
        }

        private static object? TryReadEmbeddedMessage(byte[] bytes, ProtoDecodeOptions options)
        {
            // TODO: Add clean validation instead of try/catch.
            try
            {
                return ConvertToKnownType(Decode(bytes), options);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static object ConvertToKnownType(ProtoObject protoObject, ProtoDecodeOptions options)
        {
            if (options.DecodeGuids && TryParseGuid(protoObject) is { } guid)
                return guid;

            return protoObject;
        }

        private static Guid? TryParseGuid(ProtoObject protoObject)
        {
            if (protoObject.Members.Count != 2)
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

            var version = (guidAccessor.VersionByte & 0xF0) >> 4;
            if (version is >= 1 and <= 5)
                return guidAccessor.Guid;

            return null;
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

            // Version is bits 4-7 of 8th byte.
            // See https://www.ietf.org/rfc/rfc4122.txt
            [FieldOffset(7)]
            public readonly byte VersionByte;

            public GuidAccessor(ulong low, ulong high)
            {
                Guid = default;
                VersionByte = default;
                Low = low;
                High = high;
            }
        }
    }
}
