using System.Buffers;
using ProtoBuf;
using ProtoUntyped.Decoders;
using Xunit;

namespace ProtoUntyped.Tests.Decoders;

public class ProtoDecoderTests
{
    [Theory]
    [InlineData(1, WireType.Fixed32, true)]
    [InlineData(1, WireType.Fixed64, true)]
    [InlineData(1, WireType.String, true)]
    [InlineData(1, WireType.Varint, true)]
    [InlineData(1, WireType.SignedVarint, true)]
    [InlineData(1, WireType.StartGroup, true)]
    [InlineData(1, WireType.EndGroup, false)]
    [InlineData(1, (WireType)7, false)]
    [InlineData(0, WireType.Varint, false)]
    [InlineData(0, WireType.Fixed32, false)]
    public void ShouldIdentifyValidFieldHeader(int fieldNumber, WireType wireType, bool shouldBeValid)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var writer = ProtoWriter.State.Create(bufferWriter, null);
        writer.WriteFieldHeader(fieldNumber, wireType);
        writer.Flush();

        var bytes = bufferWriter.WrittenSpan.ToArray();
        var isValid = ProtoDecoder.HasValidFieldHeader(bytes);
        
        isValid.ShouldEqual(shouldBeValid);
    }
}
