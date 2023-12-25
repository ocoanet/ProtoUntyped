using ProtoBuf;

namespace ProtoUntyped;

/// <summary>
/// Specifies how the <see cref="WireType.String"/> wire type (LEN) should be decoded.
/// </summary>
public enum StringWireTypeDecodingMode
{
    EmbeddedMessage,
    String,
    Bytes,
    PackedVarint,
    PackedFixed32,
    PackedFixed64,
}
