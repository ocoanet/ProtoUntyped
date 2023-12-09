using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ProtoBuf;

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
        if (TryReadFields(data, options, out var fields))
        {
            wireObject = new ProtoWireObject(fields);
            return true;
        }

        wireObject = default;
        return false;
    }

    private static bool TryReadFields(ReadOnlyMemory<byte> data, ProtoDecodeOptions options, out List<ProtoWireField> rawFields)
    {
        rawFields = new List<ProtoWireField>();

        var reader = ProtoReader.State.Create(data, null);
        while (reader.ReadFieldHeader() != 0)
        {
            if (!TryReadField(ref reader, options, out var field))
                return false;

            rawFields.Add(field!);
        }

        return true;
    }

    private static bool TryReadField(ref ProtoReader.State reader, ProtoDecodeOptions options, out ProtoWireField? field)
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
                field = new ProtoWireField(reader.FieldNumber, DecodeString(ref reader, options), WireType.String);
                return true;
            
            case WireType.StartGroup:
                if (TryReadGroup(ref reader, options, out var value))
                {
                    field = new ProtoWireField(reader.FieldNumber, value, WireType.StartGroup);
                    return true;
                }
                break;
        }

        field = default;
        return false;
    }
    
    private static bool TryReadGroup(ref ProtoReader.State reader, ProtoDecodeOptions options, out ProtoWireValue value)
    {
        var subItemToken = reader.StartSubItem();

        var fields = new List<ProtoWireField>();
        
        while (reader.ReadFieldHeader() != 0)
        {
            if (!TryReadField(ref reader, options, out var field))
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

    private static ProtoWireValue DecodeString(ref ProtoReader.State reader, ProtoDecodeOptions options)
    {
        // Can be a string, an embedded message or a byte array.

        var bytes = reader.AppendBytes(null);

        if (bytes.Length == 0)
            return DecodeEmptyString(options);

        if (TryReadEmbeddedMessage(bytes, options) is { } embeddedMessage)
            return embeddedMessage;

        if (!options.StringValidator.Invoke(bytes))
            return new ProtoWireValue(bytes);

        try
        {
            return new ProtoWireValue(_encoding.GetString(bytes));
        }
        catch (Exception)
        {
            return new ProtoWireValue(bytes);
        }
    }

    private static ProtoWireValue DecodeEmptyString(ProtoDecodeOptions options)
    {
        return options.EmptyStringDecodingMode switch
        {
            StringWireTypeDecodingMode.String          => new ProtoWireValue(""),
            StringWireTypeDecodingMode.EmbeddedMessage => new ProtoWireValue(ProtoWireObject.Empty),
            StringWireTypeDecodingMode.Bytes           => new ProtoWireValue(Array.Empty<byte>()),
            _                                          => throw new NotSupportedException($"Unknown string decoding mode {options.EmptyStringDecodingMode}"),
        };
    }

    private static ProtoWireValue? TryReadEmbeddedMessage(byte[] bytes, ProtoDecodeOptions options)
    {
        if (!ProtoDecoder.HasValidFieldHeader(bytes))
            // Avoids exceptions when the bytes do not start with a valid field header.
            return null;

        try
        {
            return TryReadFields(bytes, options, out var fields) ? new ProtoWireValue(new ProtoWireObject(fields)) : null;
        }
        catch (Exception)
        {
            return null;
        }
    }   
}