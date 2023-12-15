using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ProtoBuf.Serializers;

namespace ProtoUntyped.Decoders;

internal static class ProtoWireEncoder
{
    public static void Encode(IBufferWriter<byte> bufferWriter, ProtoWireObject wireObject)
    {
        var writer = ProtoWriter.State.Create(bufferWriter, null);
        WriteFields(ref writer, wireObject.Fields);
        writer.Flush();
    }
    
    public static void Encode(Stream stream, ProtoWireObject wireObject)
    {
        var writer = ProtoWriter.State.Create(stream, null);
        WriteFields(ref writer, wireObject.Fields);
        writer.Flush();
    }

    private static void WriteFields(ref ProtoWriter.State writer, IReadOnlyList<ProtoWireField> wireFields)
    {
        foreach (var wireField in wireFields)
        {
            WriteField(ref writer, wireField);
        }
    }

    private static void WriteField(ref ProtoWriter.State writer, ProtoWireField wireField)
    {
        switch (wireField.WireType)
        {
            case WireType.Varint when wireField.Value.Type == ProtoWireValueType.Int32:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.Varint);
                writer.WriteInt32(wireField.Value.Int32Value);
                break;
            
            case WireType.Varint when wireField.Value.Type == ProtoWireValueType.Int64:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.Varint);
                writer.WriteInt64(wireField.Value.Int64Value);
                break;
            
            case WireType.Fixed32 when wireField.Value.Type == ProtoWireValueType.Int32:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.Fixed32);
                writer.WriteInt32(wireField.Value.Int32Value);
                break;
            
            case WireType.Fixed64 when wireField.Value.Type == ProtoWireValueType.Int32:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.Fixed64);
                writer.WriteInt32(wireField.Value.Int32Value);
                break;
            
            case WireType.Fixed64 when wireField.Value.Type == ProtoWireValueType.Int64:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.Fixed64);
                writer.WriteInt64(wireField.Value.Int64Value);
                break;
            
            case WireType.String when wireField.Value.Type == ProtoWireValueType.String:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.String);
                writer.WriteString(wireField.Value.StringValue);
                break;
            
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Bytes:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.String);
                writer.WriteBytes(wireField.Value.BytesValue);
                break;
            
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Message:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.String);
                writer.WriteMessage(default, wireField.Value.MessageValue, WireObjectSerializer.Instance);
                break;
            
            case WireType.StartGroup:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.StartGroup);
                writer.WriteMessage(default, wireField.Value.MessageValue, WireObjectSerializer.Instance);
                break;
            
            case WireType.SignedVarint when wireField.Value.Type == ProtoWireValueType.Int32:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.SignedVarint);
                writer.WriteInt32(wireField.Value.Int32Value);
                break;
            
            case WireType.SignedVarint when wireField.Value.Type == ProtoWireValueType.Int64:
                writer.WriteFieldHeader(wireField.FieldNumber, WireType.SignedVarint);
                writer.WriteInt64(wireField.Value.Int64Value);
                break;
            
            default:
                ThrowInvalidWireTypeException(wireField);
                break;
        }
    }

    private static void ThrowInvalidWireTypeException(ProtoWireField wireField)
    {
        throw new NotSupportedException($"Unsupported wire type or value value, FieldNumber: {wireField.FieldNumber}, WireType: {wireField.WireType}, ValueType: {wireField.Value.Type}");
    }

    private class WireObjectSerializer : ISerializer<ProtoWireObject>
    {
        public static WireObjectSerializer Instance { get; } = new();
        
        private WireObjectSerializer()
        {
        }

        public SerializerFeatures Features => default;

        public ProtoWireObject Read(ref ProtoReader.State state, ProtoWireObject value)
        {
            throw new NotSupportedException();
        }

        public void Write(ref ProtoWriter.State state, ProtoWireObject value)
        {
            WriteFields(ref state, value.Fields);
        }
    }
}