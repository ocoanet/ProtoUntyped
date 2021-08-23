using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace ProtoUntyped
{
    public class ProtoObject
    {
        public List<ProtoField> Fields { get; internal set; } = new();

        public static ProtoObject Parse(ReadOnlyMemory<byte> data)
        {
            var reader = ProtoReader.State.Create(data, null);
            var root = new ProtoObject();

            while (ReadField(root, ref reader))
            {
            }
            
            return root;
        }

        private static bool ReadField(ProtoObject obj, ref ProtoReader.State reader)
        {
            if (reader.ReadFieldHeader() == 0)
                return false;

            var fieldNumber = reader.FieldNumber;
            var fieldValue = ReadFieldValue(ref reader);

            if (obj.Fields.LastOrDefault() is { } last && last.FieldNumber == fieldNumber)
                last.Add(fieldValue);
            else
                obj.Fields.Add(new ProtoField(fieldNumber, fieldValue));

            return true;
        }

        private static object ReadFieldValue(ref ProtoReader.State reader)
        {
            switch (reader.WireType)
            {
                case WireType.Varint:
                    return reader.ReadInt64();
                
                case WireType.Fixed32:
                case WireType.Fixed64:
                    // Could be an integer, assume floating point because protobuf-net defaults to varint for integers.
                    return reader.ReadDouble();
                
                case WireType.String:
                    // Can be a string or a sub message.
                    var bytes = reader.AppendBytes(null);
                    return TryReadSubMessage(bytes, out var subMessage)
                        ? subMessage
                        : Encoding.UTF8.GetString(bytes);
                
                case WireType.StartGroup:
                    break;
                
                case WireType.EndGroup:
                    break;
                
                case WireType.SignedVarint:
                    break;
            }
            
            throw new ArgumentOutOfRangeException();
        }

        private static bool TryReadSubMessage(byte[] bytes, out object message)
        {
            // TODO: Add clean validation instead of try/catch.
            try
            {
                message = Parse(bytes);
                return true;
            }
            catch (Exception)
            {
                message = default;
                return false;
            }
        }
    }
}
