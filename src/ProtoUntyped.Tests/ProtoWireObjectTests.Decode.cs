using ProtoBuf;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests;

public partial class ProtoWireObjectTests
{
    public class Decode
    {
        [Fact]
        public void ShouldParseSimpleMessage()
        {
            var message = ThreadLocalFixture.Create<SearchRequest>();

            var bytes = ProtoBufUtil.Serialize(message);
            var wireObject = ProtoWireObject.Decode(bytes);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, message.Query),
                new(2, (long)message.PageNumber),
                new(3, (long)message.ResultPerPage)
            ));
        }

        [Fact]
        public void ShouldParseMessageWithFloats()
        {
            var message = ThreadLocalFixture.Create<MessageWithFloats>();

            var bytes = ProtoBufUtil.Serialize(message);
            var wireObject = ProtoWireObject.Decode(bytes);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, message.DoubleValue),
                new(2, message.SingleValue)
            ));
        }

        [Fact]
        public void ShouldParseMessageWithIntegers()
        {
            var message = ThreadLocalFixture.Create<MessageWithIntegers>();

            var bytes = ProtoBufUtil.Serialize(message);
            var wireObject = ProtoWireObject.Decode(bytes);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, (long)message.Int16Value),
                new(2, (long)message.UInt16Value),
                new(3, (long)message.Int32Value),
                new(4, (long)message.UInt32Value),
                new(5, (long)message.Int64Value),
                new(6, (long)message.UInt64Value),
                new(7, (long)message.ByteValue),
                new(8, (long)message.SByteValue)
            ));
        }

        [Fact]
        public void ShouldParseMessageWithNestedTypes()
        {
            var message = ThreadLocalFixture.Create<MessageWithNestedTypes>();

            var bytes = ProtoBufUtil.Serialize(message);
            var wireObject = ProtoWireObject.Decode(bytes);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, (long)message.Id),
                new(2, new ProtoWireObject(
                    new ProtoWireField(1, new ProtoWireObject(
                        new(1, (long)message.Nested1.Nested2.Value1),
                        new(2, message.Nested1.Nested2.Value2)
                    ))
                )),
                new(3, message.Key)
            ));
        }

        [Fact]
        public void ShouldParseMessageWithGroups()
        {
            var message = ThreadLocalFixture.Create<MessageWithGroups>();

            var bytes = ProtoBufUtil.Serialize(message);
            var wireObject = ProtoWireObject.Decode(bytes);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, (long)message.Id),
                new(2, WireType.StartGroup, new ProtoWireObject(
                    new ProtoWireField(1, WireType.StartGroup, new ProtoWireObject(
                        new(1, (long)message.Nested1.Nested2.Value1),
                        new(2, message.Nested1.Nested2.Value2)
                    ))
                )),
                new(3, message.Key)
            ));
        }

        [Fact]
        public void ShouldParseMessageWithArrays()
        {
            var message = new MessageWithArrays
            {
                Id = 123,
                Int32Array = new[]
                {
                    101,
                    102,
                    103,
                },
                MessageArray = new[]
                {
                    new MessageWithArrays.Nested { Id = 1001, Key = "ABC1" },
                    new MessageWithArrays.Nested { Id = 1002, Key = "ABC2" },
                },
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var wireObject = ProtoWireObject.Decode(bytes);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, 123L),
                new(2, 101L),
                new(2, 102L),
                new(2, 103L),
                new(3, new ProtoWireObject(
                    new(1, 1001L),
                    new(2, "ABC1")
                )),
                new(3, new ProtoWireObject(
                    new(1, 1002L),
                    new(2, "ABC2")
                ))
            ));
        }
        
        [Fact]
        public void ShouldParseMessageWithPackedVarintArrays()
        {
            var message = new MessageWithPackedVarintArray
            {
                Id = 123,
                Int32Array = [101, 102, 103],
                Int64Array = [201L, 202L],
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var decodeOptions = new ProtoDecodeOptions { PreferredStringDecodingModes = new[] { StringWireTypeDecodingMode.PackedVarint } };
            var wireObject = ProtoWireObject.Decode(bytes, decodeOptions);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, 123L),
                new(2, WireType.Varint, [101L, 102L, 103L]),
                new(3, WireType.Varint, [201L, 202L])
            ));
        }
        
        [Fact]
        public void ShouldParseMessageWithPackedFixed32Arrays()
        {
            var message = new MessageWithPackedFixed32Array
            {
                Id = 123,
                Int32Array = [101, 102, 103],
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var decodeOptions = new ProtoDecodeOptions { PreferredStringDecodingModes = new[] { StringWireTypeDecodingMode.PackedFixed32 } };
            var wireObject = ProtoWireObject.Decode(bytes, decodeOptions);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, 123L),
                new(2, WireType.Fixed32, new[] { 101, 102, 103 })
            ));
        }
        
        [Fact]
        public void ShouldParseMessageWithPackedFixed64Arrays()
        {
            var message = new MessageWithPackedFixed64Array
            {
                Id = 123,
                Int64Array = [101L, 102L, 103L]
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var decodeOptions = new ProtoDecodeOptions { PreferredStringDecodingModes = new[] { StringWireTypeDecodingMode.PackedFixed64 } };
            var wireObject = ProtoWireObject.Decode(bytes, decodeOptions);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, 123L),
                new(2, WireType.Fixed64, [101L, 102L, 103L])
            ));
        }

        [Theory]
        [InlineData("ABC")]
        [InlineData("ABC 123")]
        [InlineData("ABC 123\\789.+=#")]
        public void ShouldParseMessageWithString(string s)
        {
            var message = new MessageWithString
            {
                Id = 42,
                Data = s,
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var wireObject = ProtoWireObject.Decode(bytes);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, (long)message.Id),
                new(2, message.Data)
            ));
        }

        [Theory]
        [InlineData(new byte[] { 0 })]
        [InlineData(new byte[] { 1 })]
        [InlineData(new byte[] { 2 })]
        [InlineData(new byte[] { 3 })]
        [InlineData(new byte[] { 4 })]
        [InlineData(new byte[] { 5 })]
        [InlineData(new byte[] { 200 })]
        public void ShouldParseMessageWithBytes(byte[] data)
        {
            var message = new MessageWithByteArray
            {
                Id = 42,
                Data = data,
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var wireObject = ProtoWireObject.Decode(bytes);

            wireObject.ShouldDeepEqual(new ProtoWireObject(
                new(1, (long)message.Id),
                new(2, message.Data)
            ));
        }
    }
}