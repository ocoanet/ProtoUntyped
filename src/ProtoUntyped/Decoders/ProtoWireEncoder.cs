﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using ProtoBuf.Serializers;

namespace ProtoUntyped.Decoders;

internal static class ProtoWireEncoder
{
    public static bool CanBeEncoded(ProtoWireObject wireObject)
    {
        return wireObject.Fields.All(IsEncodingSupported);
    }
    
    public static bool IsEncodingSupported(ProtoWireField wireField)
    {
        if (wireField.FieldNumber <= 0)
            return false;
        
        switch (wireField.WireType)
        {
            case WireType.Varint when wireField.Value.Type == ProtoWireValueType.Int32:
            case WireType.Varint when wireField.Value.Type == ProtoWireValueType.Int64:
            case WireType.Fixed32 when wireField.Value.Type == ProtoWireValueType.Int32:
            case WireType.Fixed64 when wireField.Value.Type == ProtoWireValueType.Int32:
            case WireType.Fixed64 when wireField.Value.Type == ProtoWireValueType.Int64:
            case WireType.String when wireField.Value.Type == ProtoWireValueType.String:
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Bytes:
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Int32Array && wireField.PackedWireType == WireType.Fixed32:
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Int64Array && wireField.PackedWireType == WireType.Fixed64:
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Int64Array && wireField.PackedWireType == WireType.Varint:
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Int64Array && wireField.PackedWireType == WireType.SignedVarint:
            case WireType.SignedVarint when wireField.Value.Type == ProtoWireValueType.Int32:
            case WireType.SignedVarint when wireField.Value.Type == ProtoWireValueType.Int64:
                return true;
            
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Message:
            case WireType.StartGroup when wireField.Value.Type == ProtoWireValueType.Message:
                return CanBeEncoded(wireField.Value.MessageValue);

            default:
                return false;
        }
    }
    
    public static void Encode(IBufferWriter<byte> bufferWriter, ProtoWireObject wireObject)
    {
        var writer = ProtoWriter.State.Create(bufferWriter, null);
        EncodeFields(ref writer, wireObject.Fields);
        writer.Flush();
    }
    
    public static void Encode(Stream stream, ProtoWireObject wireObject)
    {
        var writer = ProtoWriter.State.Create(stream, null);
        EncodeFields(ref writer, wireObject.Fields);
        writer.Flush();
    }

    private static void EncodeFields(ref ProtoWriter.State writer, IReadOnlyList<ProtoWireField> wireFields)
    {
        foreach (var wireField in wireFields)
        {
            EncodeField(ref writer, wireField);
        }
    }

    private static void EncodeField(ref ProtoWriter.State writer, ProtoWireField wireField)
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
            
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Int32Array && wireField.PackedWireType == WireType.Fixed32:
                EncodePackedFieldValue(ref writer, wireField.FieldNumber, SerializerFeatures.WireTypeFixed32, PackedValueSerializer<int>.Instance, wireField.Value.Int32ArrayValue);
                break;
            
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Int64Array && wireField.PackedWireType == WireType.Fixed64:
                EncodePackedFieldValue(ref writer, wireField.FieldNumber, SerializerFeatures.WireTypeFixed64, PackedValueSerializer<long>.Instance, wireField.Value.Int64ArrayValue);
                break;
            
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Int64Array && wireField.PackedWireType == WireType.Varint:
                EncodePackedFieldValue(ref writer, wireField.FieldNumber, SerializerFeatures.WireTypeVarint, PackedValueSerializer<long>.Instance, wireField.Value.Int64ArrayValue);
                break;
            
            case WireType.String when wireField.Value.Type == ProtoWireValueType.Int64Array && wireField.PackedWireType == WireType.SignedVarint:
                EncodePackedFieldValue(ref writer, wireField.FieldNumber, SerializerFeatures.WireTypeSignedVarint, PackedValueSerializer<long>.Instance, wireField.Value.Int64ArrayValue);
                break;
            
            case WireType.StartGroup when wireField.Value.Type == ProtoWireValueType.Message:
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

    private static void EncodePackedFieldValue<T>(ref ProtoWriter.State writer, int fieldNumber, SerializerFeatures wireType, ISerializer<T> serializer, T[] values)
    {
        var repeatedSerializer = RepeatedSerializer.CreateVector<T>();
        repeatedSerializer.WriteRepeated(ref writer, fieldNumber, wireType, values, serializer);
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
            EncodeFields(ref state, value.Fields);
        }
    }
}