using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ProtoBuf;
using ProtoBuf.Serializers;

namespace ProtoUntyped.Decoders;

internal static class ProtoWireDecoder
{
    private static readonly Encoding _encoding = new UTF8Encoding(true, true);
    
#if NETSTANDARD2_1
    public static bool TryDecode(ReadOnlyMemory<byte> data, ProtoDecodeOptions options, [NotNullWhen(true)] out ProtoWireObject? wireObject)
#else
    public static bool TryDecode(ReadOnlyMemory<byte> data, ProtoDecodeOptions options, out ProtoWireObject? wireObject)
#endif
    {
        if (TryDecodeFields(data, options, out var fields))
        {
            wireObject = new ProtoWireObject(fields);
            return true;
        }

        wireObject = default;
        return false;
    }

    private static bool TryDecodeFields(ReadOnlyMemory<byte> data, ProtoDecodeOptions options, out List<ProtoWireField> wireFields)
    {
        wireFields = new List<ProtoWireField>();

        var reader = ProtoReader.State.Create(data, null);
        while (reader.ReadFieldHeader() != 0)
        {
            if (!TryDecodedField(ref reader, options, out var field))
                return false;

            wireFields.Add(field!);
        }

        return true;
    }

    private static bool TryDecodedField(ref ProtoReader.State reader, ProtoDecodeOptions options, out ProtoWireField? field)
    {
        switch (reader.WireType)
        {
            case WireType.Varint:
                field = new ProtoWireField(reader.FieldNumber, reader.ReadInt64(), WireType.Varint);
                return true;

            case WireType.Fixed32:
                field = new ProtoWireField(reader.FieldNumber, reader.ReadInt32(), WireType.Fixed32);
                return true;

            case WireType.Fixed64:
                field = new ProtoWireField(reader.FieldNumber, reader.ReadInt64(), WireType.Fixed64);
                return true;

            case WireType.String:
                field = DecodeStringField(ref reader, options);
                return true;
            
            case WireType.StartGroup:
                if (TryDecodeGroup(ref reader, options, out var value))
                {
                    field = new ProtoWireField(reader.FieldNumber, value, WireType.StartGroup);
                    return true;
                }
                break;
        }

        field = default;
        return false;
    }
    
    private static bool TryDecodeGroup(ref ProtoReader.State reader, ProtoDecodeOptions options, out ProtoWireValue value)
    {
        var subItemToken = reader.StartSubItem();

        var fields = new List<ProtoWireField>();
        
        while (reader.ReadFieldHeader() != 0)
        {
            if (!TryDecodedField(ref reader, options, out var field))
            {
                value = default;
                return false;
            }

            fields.Add(field!);
        }
        
        reader.EndSubItem(subItemToken);

        value = new ProtoWireValue(new ProtoWireObject(fields));
        
        return true;
    }

    private static ProtoWireField DecodeStringField(ref ProtoReader.State reader, ProtoDecodeOptions options)
    {
        var fieldNumber = reader.FieldNumber;
        var bytes = reader.AppendBytes(null);

        foreach (var stringDecodingMode in options.PreferredStringDecodingModes)
        {
            switch (stringDecodingMode)
            {
                case StringWireTypeDecodingMode.EmbeddedMessage:
                    if (options.EmbeddedMessageValidator.Invoke(bytes) && TryDecodeEmbeddedMessage(bytes, options, out var embeddedMessage))
                        return new ProtoWireField(fieldNumber, embeddedMessage!);
                    break;
                    
                case StringWireTypeDecodingMode.String:
                    if (options.StringValidator.Invoke(bytes) && TryDecodeString(bytes, out var s))
                        return new ProtoWireField(fieldNumber, s);
                    break;
                    
                case StringWireTypeDecodingMode.Bytes:
                    return new ProtoWireField(fieldNumber, bytes);
                
                case StringWireTypeDecodingMode.PackedVarint:
                    if (TryDecodePackedFieldValue(bytes, SerializerFeatures.WireTypeVarint, PackedValueSerializer<long>.Instance, out var packedVarintValues))
                        return new ProtoWireField(fieldNumber, WireType.Varint, packedVarintValues);
                    break;
                
                case StringWireTypeDecodingMode.PackedFixed32:
                    if (TryDecodePackedFieldValue(bytes, SerializerFeatures.WireTypeFixed32, PackedValueSerializer<int>.Instance, out var packedInt32Values))
                        return new ProtoWireField(fieldNumber, WireType.Fixed32, packedInt32Values);
                    break;
                
                case StringWireTypeDecodingMode.PackedFixed64:
                    if (TryDecodePackedFieldValue(bytes, SerializerFeatures.WireTypeFixed64, PackedValueSerializer<long>.Instance, out var packedInt64Values))
                        return new ProtoWireField(fieldNumber, WireType.Fixed64, packedInt64Values);
                    break;
                
                default:
                    throw new NotSupportedException($"Unknown decoding mode: {stringDecodingMode}");
            }
        }

        return new ProtoWireField(fieldNumber, bytes);
    }

    private static bool TryDecodeEmbeddedMessage(byte[] bytes, ProtoDecodeOptions options, out ProtoWireObject? embeddedMessage)
    {
        if (bytes.Length == 0)
        {
            embeddedMessage = ProtoWireObject.Empty;
            return true;
        }
        
        try
        {
            if (ProtoDecoder.HasValidFieldHeader(bytes) && TryDecodeFields(bytes, options, out var fields))
            {
                embeddedMessage = new ProtoWireObject(fields);
                return true;
            }
        }
        catch
        {
            // TryReadFields can throw if the bytes are not a valid message.
        }

        embeddedMessage = null;
        return false;
    }

    private static bool TryDecodeString(byte[] bytes, out string s)
    {
        try
        {
            s = _encoding.GetString(bytes);
            return true;
        }
        catch (Exception)
        {
            s = null!;
            return false;
        }
    }

    private static bool TryDecodePackedFieldValue<T>(byte[] bytes, SerializerFeatures wireType, ISerializer<T> serializer, out T[] values)
    {
        var tagAndLength = new byte[6];
        tagAndLength[0] = (1 << 3) | (byte)WireType.String;
        var headerLength = 1 + WriteVarint64((ulong)bytes.Length, tagAndLength.AsSpan(1));
        
        var segment1 = new MemorySegment<byte>(tagAndLength.AsMemory(0, headerLength));
        var segment2 = segment1.Append(bytes);
        var sequence = new ReadOnlySequence<byte>(segment1, 0, segment2, segment2.Memory.Length);
        
        var reader = ProtoReader.State.Create(sequence, null);

        reader.ReadFieldHeader();
        
        var repeatedSerializer = RepeatedSerializer.CreateVector<T>();
        try
        {
            values = repeatedSerializer.ReadRepeated(ref reader, wireType, null, serializer);
        }
        catch
        {
            values = null!;
            return false;
        }
        
        return true;
    }
    
    private static int WriteVarint64(ulong value, Span<byte> span)
    {
        var count = 0;
        do
        {
            span[count++] = (byte)((value & 0x7F) | 0x80);
        } while ((value >>= 7) != 0);
        span[count - 1] &= 0x7F;
        return count;
    }

    private class MemorySegment<T> : ReadOnlySequenceSegment<T>
    {
        public MemorySegment(ReadOnlyMemory<T> memory)
        {
            Memory = memory;
        }

        public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
        {
            var segment = new MemorySegment<T>(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };

            Next = segment;

            return segment;
        }
    }
}