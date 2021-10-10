using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using ProtoUntyped.Decoders;

namespace ProtoUntyped
{
    public class ProtoObject
    {
        private static readonly Encoding _encoding = new UTF8Encoding(true, true);
        
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
            
            try
            {
                var s = _encoding.GetString(bytes);
                return options.StringValidator.Invoke(s) ? s : bytes;
            }
            catch (Exception)
            {
                return bytes;
            }
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
            if (options.DecodeGuid && GuidDecoder.TryParseGuid(protoObject, options) is { } guid)
                return guid;

            if (options.DecodeDateTime && TimeDecoder.TryParseDateTime(protoObject, options) is { } dateTime)
                return dateTime;
            
            if (options.DecodeTimeSpan && TimeDecoder.TryParseTimeSpan(protoObject, options) is { } timeSpan)
                return timeSpan;

            return protoObject;
        }
       
    }
}
