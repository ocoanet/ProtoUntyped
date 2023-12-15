using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests;

public partial class ProtoWireObjectTests
{
    public class Encode
    {
        [Fact]
        public void ShouldEncodeTestMessageToArray()
        {
            var message = ThreadLocalFixture.Create<TestMessage>();
            var bytes = ProtoBufUtil.Serialize(message);

            var wireObject = ProtoWireObject.Decode(bytes);
            var encodedBytes = wireObject.Encode().ToArray();

            encodedBytes.ShouldEqual(bytes);
        }

        [Fact]
        public void ShouldEncodeTestMessageToStream()
        {
            var message = ThreadLocalFixture.Create<TestMessage>();
            var bytes = ProtoBufUtil.Serialize(message);

            var wireObject = ProtoWireObject.Decode(bytes);
            var stream = new MemoryStream();
            wireObject.EncodeTo(stream);

            var encodedBytes = stream.ToArray();
            encodedBytes.ShouldEqual(bytes);
        }

        [Fact]
        public void ShouldEncodeTestMessageToBufferWriter()
        {
            var message = ThreadLocalFixture.Create<TestMessage>();
            var bytes = ProtoBufUtil.Serialize(message);

            var wireObject = ProtoWireObject.Decode(bytes);
            var bufferWriter = new ArrayBufferWriter<byte>();
            wireObject.EncodeTo(bufferWriter);

            var encodedBytes = bufferWriter.WrittenSpan.ToArray();
            encodedBytes.ShouldEqual(bytes);
        }

        [Theory]
        [MemberData(nameof(CreateMessagesData))]
        public void ShouldEncodeMessage(object message)
        {
            var bytes = ProtoBufUtil.Serialize(message);

            var wireObject = ProtoWireObject.Decode(bytes);
            var encodedBytes = wireObject.Encode().ToArray();

            encodedBytes.ShouldEqual(bytes);
        }

        public static IEnumerable<object[]> CreateMessagesData()
        {
            foreach (var messageType in typeof(MessageWithArrays).Assembly.GetTypes().Where(x => Attribute.IsDefined(x, typeof(ProtoContractAttribute))))
            {
                yield return new object[] { ThreadLocalFixture.Create(messageType) };
            }
        }

        [ProtoContract]
        public class TestMessage
        {
            [ProtoMember(1)] public int Id { get; set; }

            [ProtoMember(2)] public string Name { get; set; }

            [ProtoMember(3)] public decimal Amount { get; set; }

            [ProtoMember(4)] public TestNestedMessage NestedMessage { get; set; }
        }

        [ProtoContract]
        public class TestNestedMessage
        {
            [ProtoMember(1)] public string Key { get; set; }

            [ProtoMember(2)] public List<decimal> Values { get; set; }
        }
    }
}