using ProtoBuf;

namespace ProtoUntyped;

/// <summary>
/// Specifies how the <see cref="WireType.String"/> wire type (LEN) should be decoded.
/// </summary>
public enum StringWireTypeDecodingMode
{
    String,
    EmbeddedMessage,
    Bytes,
}
