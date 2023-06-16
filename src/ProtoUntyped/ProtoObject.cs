using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ProtoBuf;
using ProtoUntyped.Decoders;

namespace ProtoUntyped;

[DebuggerDisplay("{" + nameof(ProtoUntypedDebuggerDisplay) + "." + nameof(ProtoUntypedDebuggerDisplay.GetDebugString) + "(this)}")]
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

    public void SortMembers()
    {
        Members.Sort((x, y) => x.FieldNumber.CompareTo(y.FieldNumber));
    }
    
    public void SortMembers(bool recursive)
    {
        if (recursive)
            Accept(ProtoObjectMemberSorter.Instance);
        else
            SortMembers();
    }

    public void Accept(ProtoObjectVisitor visitor)
    {
        visitor.Visit(this);

        foreach (var member in Members)
        {
            member.Accept(visitor);
        }
    }

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
        return ToString(ProtoFormatter.Default);
    }

    public string ToString(ProtoFormatter formatter)
    {
        return formatter.BuildString(this);
    }

    public static ProtoObject Decode(ReadOnlyMemory<byte> data)
    {
        return Decode(data, new ProtoDecodeOptions());
    }

    public static ProtoObject Decode(ReadOnlyMemory<byte> data, ProtoDecodeOptions options)
    {
        if (TryDecode(data, options, out var protoObject))
            return protoObject!;

        throw new ArgumentException("Unable to parse proto-object");
    }

#if NETSTANDARD2_1
    public static bool TryDecode(ReadOnlyMemory<byte> data, ProtoDecodeOptions options, [NotNullWhen(true)] out ProtoObject? protoObject)
#else
        public static bool TryDecode(ReadOnlyMemory<byte> data, ProtoDecodeOptions options, out ProtoObject? protoObject)
#endif
    {
        if (TryReadFields(data, options, out var fields))
        {
            protoObject = new ProtoObject(GroupRepeatedFields(fields));
            return true;
        }

        protoObject = default;
        return false;
    }

    private static List<ProtoMember> GroupRepeatedFields(List<ProtoField> fields)
    {
        return fields.GroupBy(x => x.FieldNumber)
                     .Select(g => g.Count() == 1 ? (ProtoMember)g.Single() : new ProtoArray(g.Key, g.Select(x => new ProtoValue(x.Value, x.WireType)).ToArray()))
                     .ToList();
    }

    private static bool TryReadFields(ReadOnlyMemory<byte> data, ProtoDecodeOptions options, out List<ProtoField> fields)
    {
        fields = new List<ProtoField>();

        var reader = ProtoReader.State.Create(data, null);
        while (reader.ReadFieldHeader() != 0)
        {
            if (!TryReadField(ref reader, options, out var field))
                return false;

            fields.Add(field!);
        }

        return true;
    }

    private static bool TryReadField(ref ProtoReader.State reader, ProtoDecodeOptions options, out ProtoField? field)
    {
        var fieldNumber = reader.FieldNumber;
        var wireType = reader.WireType;

        if (TryReadFieldValue(ref reader, options, out var fieldValue))
        {
            field = new ProtoField(fieldNumber, fieldValue!, wireType);
            return true;
        }

        field = default;
        return false;
    }

    private static bool TryReadFieldValue(ref ProtoReader.State reader, ProtoDecodeOptions options, out object? value)
    {
        switch (reader.WireType)
        {
            case WireType.Varint:
                value = reader.ReadInt64();
                break;

            case WireType.Fixed32:
                // Could be an integer, assume floating point because protobuf-net defaults to varint for integers.
                value = reader.ReadSingle();
                break;

            case WireType.Fixed64:
                // Could be an integer, assume floating point because protobuf-net defaults to varint for integers.
                value = reader.ReadDouble();
                break;

            case WireType.String:
                value = DecodeString(ref reader, options);
                break;

            default:
                value = default;
                return false;
        }

        return true;
    }

    private static object DecodeString(ref ProtoReader.State reader, ProtoDecodeOptions options)
    {
        // Can be a string, an embedded message or a byte array.

        var bytes = reader.AppendBytes(null);

        if (bytes.Length == 0)
            return DecodeEmptyString(options);

        if (TryReadEmbeddedMessage(bytes, options) is { } embeddedMessage)
            return embeddedMessage;

        if (!options.StringValidator.Invoke(bytes))
            return bytes;

        try
        {
            return _encoding.GetString(bytes);
        }
        catch (Exception)
        {
            return bytes;
        }
    }

    private static object DecodeEmptyString(ProtoDecodeOptions options)
    {
        return options.EmptyStringDecodingMode switch
        {
            StringWireTypeDecodingMode.String          => "",
            StringWireTypeDecodingMode.EmbeddedMessage => new ProtoObject(),
            StringWireTypeDecodingMode.Bytes           => Array.Empty<byte>(),
            _                                          => throw new NotSupportedException($"Unknown string decoding mode {options.EmptyStringDecodingMode}"),
        };
    }

    private static object? TryReadEmbeddedMessage(byte[] bytes, ProtoDecodeOptions options)
    {
        if (!ProtoDecoder.HasValidFieldHeader(bytes))
            // Avoids exceptions when the bytes do not start with a valid field header.
            return null;

        try
        {
            return TryDecode(bytes, options, out var protoObject) ? ConvertToKnownType(protoObject!, options) : null;
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

        if (options.DecodeDecimal && DecimalDecoder.TryParseDecimal(protoObject, options) is { } dec)
            return dec;

        return protoObject;
    }
}