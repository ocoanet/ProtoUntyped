using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ProtoBuf;
using ProtoUntyped.Decoders;

namespace ProtoUntyped;

/// <summary>
/// Represents a raw protobuf message read from the wire format.
/// </summary>
/// <remarks>
/// The goal of the <see cref="ProtoWireObject"/> is to be as close as possible to the wire
/// format. However, decoding <see cref="ProtoWireObject"/> instances still requires to
/// apply heuristics to parse <see cref="WireType.String"/> fields, which can be either
/// strings, byte arrays or nested messages.
/// </remarks>
[DebuggerTypeProxy(typeof(ProtoWireObjectDebugView))]
[DebuggerDisplay("{" + nameof(ProtoUntypedDebuggerDisplay) + "." + nameof(ProtoUntypedDebuggerDisplay.GetDebugString) + "(this)}")]
public class ProtoWireObject
{
    public static readonly ProtoWireObject Empty = new(Array.Empty<ProtoWireField>());
    
    public ProtoWireObject(params ProtoWireField[] fields)
        : this(fields.ToList())
    {
    }
    
    public ProtoWireObject(IReadOnlyList<ProtoWireField> fields)
    {
        Fields = fields;
    }
    
    public IReadOnlyList<ProtoWireField> Fields { get; }

    public static ProtoWireObject Decode(ReadOnlyMemory<byte> data)
    {
        return Decode(data, new ProtoDecodeOptions());
    }
    
    public static ProtoWireObject Decode(ReadOnlyMemory<byte> data, ProtoDecodeOptions decodeOptions)
    {
        if (TryDecode(data, decodeOptions, out var protoObject))
            return protoObject!;

        throw new ArgumentException("Unable to parse object from wire format");
    }
    
#if NETSTANDARD2_1
    public static bool TryDecode(ReadOnlyMemory<byte> data, ProtoDecodeOptions decodeOptions, [NotNullWhen(true)] out ProtoWireObject? wireObject)
#else
    public static bool TryDecode(ReadOnlyMemory<byte> data, ProtoDecodeOptions decodeOptions, out ProtoWireObject? wireObject)
#endif
    {
        return ProtoWireDecoder.TryDecode(data, decodeOptions, out wireObject);
    }

    public ProtoObject ToProtoObject(ProtoDecodeOptions decodeOptions)
    {
        return ProtoDecoder.Decode(this, decodeOptions);
    }

    public string ToProtoscopeString()
    {
        return ProtoscopeFormatter.Default.BuildString(this);
    }
}