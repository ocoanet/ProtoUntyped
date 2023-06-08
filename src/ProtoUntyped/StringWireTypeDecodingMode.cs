namespace ProtoUntyped;

/// <summary>
/// Specifies how the string wire type (LEN) is decoded.
/// </summary>
public enum StringWireTypeDecodingMode
{
    String,
    EmbeddedMessage,
    Bytes,
}
