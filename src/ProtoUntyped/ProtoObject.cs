using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ProtoUntyped.Decoders;

namespace ProtoUntyped;

/// <summary>
/// Represents a decoded protobuf message read.
/// </summary>
/// <remarks>
/// The goal of the <see cref="ProtoObject"/> is to be as close as possible to the serialized object.
/// </remarks>
[DebuggerTypeProxy(typeof(ProtoObjectDebugView))]
[DebuggerDisplay("{" + nameof(ProtoUntypedDebuggerDisplay) + "." + nameof(ProtoUntypedDebuggerDisplay.GetDebugString) + "(this)}")]
public class ProtoObject
{
    public static readonly ProtoObject Empty = new ProtoObject(Array.Empty<ProtoField>());
    
    private readonly List<ProtoField> _fields;
    private readonly ILookup<int, ProtoField> _fieldsLookup;
    
    public ProtoObject(params ProtoField[] fields)
        : this(fields.ToList())
    {
    }

    public ProtoObject(List<ProtoField> fields)
    {
        _fields = fields;
        _fieldsLookup = fields.ToLookup(x => x.FieldNumber);
    }

    public IReadOnlyList<ProtoField> Fields => _fields;

    public IEnumerable<ProtoField> GetFields(int fieldNumber) => _fieldsLookup[fieldNumber];

    public void SortFields()
    {
        _fields.Sort((x, y) => x.FieldNumber.CompareTo(y.FieldNumber));
    }
    
    public void SortFields(bool recursive)
    {
        SortFields();

        if (recursive)
        {
            foreach (var protoObject in _fields.SelectMany(x => x.GetValues()).OfType<ProtoObject>())
            {
                protoObject.SortFields(true);
            }
        }
    }

    public Dictionary<int, object> ToFieldDictionary()
    {
        return Fields.ToDictionary(x => x.FieldNumber, x => ConvertValue(x.Value));

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

    public static ProtoObject Decode(ReadOnlyMemory<byte> data, ProtoDecodeOptions decodeOptions)
    {
        if (TryDecode(data, decodeOptions, out var protoObject))
            return protoObject!;

        throw new ArgumentException("Unable to parse object from wire format");
    }

#if NETSTANDARD2_1
    public static bool TryDecode(ReadOnlyMemory<byte> data, ProtoDecodeOptions decodeOptions, [NotNullWhen(true)] out ProtoObject? protoObject)
#else
    public static bool TryDecode(ReadOnlyMemory<byte> data, ProtoDecodeOptions decodeOptions, out ProtoObject? protoObject)
#endif
    {
        return ProtoDecoder.TryDecode(data, decodeOptions, out protoObject);
    }

    public static ProtoObject Decode(ProtoWireObject wireObject, ProtoDecodeOptions decodeOptions)
    {
        return ProtoDecoder.Decode(wireObject, decodeOptions);
    }
}