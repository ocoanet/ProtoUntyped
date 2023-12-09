using ProtoBuf;

namespace ProtoUntyped;

/// <summary>
/// Specifies how the <see cref="WireType.Fixed32"/> and <see cref="WireType.Fixed64"/> wire types should be decoded.
/// </summary>
public enum FixedWireTypeDecodingMode
{
    /// <summary>
    /// Decode <see cref="WireType.Fixed32"/> and <see cref="WireType.Fixed64"/> as <see cref="System.Single"/> and <see cref="System.Double"/>, respectively.
    /// </summary>
    /// <remarks>
    /// This is the default value because protobuf-net defaults to varint for integers and uses I32/I64 for floats/doubles.
    /// </remarks>
    FloatingPoint,
    /// <summary>
    /// Decode <see cref="WireType.Fixed32"/> and <see cref="WireType.Fixed64"/> as <see cref="System.Int32"/> and <see cref="System.Int64"/>, respectively.
    /// </summary>
    Integer,
}