using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests;

public partial class ProtoObjectTests
{
    public class Encode
    {
        [Fact]
        public void ShouldEncodeTestMessage()
        {
            var protoObject = new ProtoObject(
                new ProtoField(1, "/users"),
                new ProtoField(2, 8, WireType.Varint),
                new ProtoField(3, 50, WireType.Varint)
            );

            var bytes = protoObject.Encode().ToArray();

            var expectedBytes = ProtoBufUtil.Serialize(new SearchRequest
            {
                Query = "/users",
                PageNumber = 8,
                ResultPerPage = 50,
            });
            
            bytes.ShouldEqual(expectedBytes);
        }
        
        [Fact]
        public void ShouldEncodeTestMessageToArray()
        {
            var message = ThreadLocalFixture.Create<TestMessage>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, message.GetRequiredDecodeOptions());
            var encodedBytes = protoObject.Encode().ToArray();

            encodedBytes.ShouldEqual(bytes);
        }

        [Fact]
        public void ShouldEncodeTestMessageToStream()
        {
            var message = ThreadLocalFixture.Create<TestMessage>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, message.GetRequiredDecodeOptions());
            var stream = new MemoryStream();
            protoObject.EncodeTo(stream);

            var encodedBytes = stream.ToArray();
            encodedBytes.ShouldEqual(bytes);
        }

        [Fact]
        public void ShouldEncodeTestMessageToBufferWriter()
        {
            var message = ThreadLocalFixture.Create<TestMessage>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, message.GetRequiredDecodeOptions());
            var bufferWriter = new ArrayBufferWriter<byte>();
            protoObject.EncodeTo(bufferWriter);

            var encodedBytes = bufferWriter.WrittenSpan.ToArray();
            encodedBytes.ShouldEqual(bytes);
        }

        [Theory]
        [MemberData(nameof(CreateMessagesData))]
        public void ShouldEncodeMessage(object message)
        {
            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes, TestData.GetDecodeOptions(message));

            var canBeEncoded = protoObject.CanBeEncoded();
            canBeEncoded.ShouldEqual(true);

            var encodedBytes = protoObject.Encode().ToArray();
            encodedBytes.ShouldEqual(bytes);
        }

        public static IEnumerable<object[]> CreateMessagesData()
        {
            return TestData.CreateTestMessages().Select(x => new[] { x });
        }
    }
}