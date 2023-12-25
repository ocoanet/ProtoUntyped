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

            var canBeEncoded = wireObject.CanBeEncoded();
            canBeEncoded.ShouldEqual(true);

            var encodedBytes = wireObject.Encode().ToArray();
            encodedBytes.ShouldEqual(bytes);
        }

        [Theory]
        [MemberData(nameof(GetInvalidWireFieldsData))]
        public void ShouldNotEncodeInvalidWireField(ProtoWireField wireField)
        {
            var wireObject = new ProtoWireObject(wireField);

            var canBeEncoded = wireObject.CanBeEncoded();

            canBeEncoded.ShouldEqual(false);
        }

        public static IEnumerable<object[]> GetInvalidWireFieldsData()
        {
            // Invalid field number
            yield return new object[] { new ProtoWireField(-1, 1) };
            yield return new object[] { new ProtoWireField(0, 1) };
            // Invalid wire type for Int32
            var int32Value = new ProtoWireValue(1);
            yield return new object[] { new ProtoWireField(1, int32Value, WireType.None) };
            yield return new object[] { new ProtoWireField(1, int32Value, WireType.String) };
            yield return new object[] { new ProtoWireField(1, int32Value, WireType.StartGroup) };
            yield return new object[] { new ProtoWireField(1, int32Value, WireType.EndGroup) };
            // Invalid wire type for Int64
            var int64Value = new ProtoWireValue(1L);
            yield return new object[] { new ProtoWireField(1, int64Value, WireType.None) };
            yield return new object[] { new ProtoWireField(1, int64Value, WireType.String) };
            yield return new object[] { new ProtoWireField(1, int64Value, WireType.StartGroup) };
            yield return new object[] { new ProtoWireField(1, int64Value, WireType.EndGroup) };
            yield return new object[] { new ProtoWireField(1, int64Value, WireType.Fixed32) };
            // Invalid wire type for Int32 array
            var int32ArrayValue = new ProtoWireValue(new[] { 1 });
            foreach (var wireType in Enum.GetValues<WireType>().Where(x => x != WireType.String))
            {
                yield return new object[] { new ProtoWireField(1, int32ArrayValue, wireType) };
            }
            foreach (var packedWireType in Enum.GetValues<WireType>().Where(x => x != WireType.Fixed32))
            {
                yield return new object[] { new ProtoWireField(1, int32ArrayValue, WireType.String, packedWireType) };
            }
            // Invalid wire type for Int64 array
            var int64ArrayValue = new ProtoWireValue(new[] { 1L });
            foreach (var wireType in Enum.GetValues<WireType>().Where(x => x != WireType.String))
            {
                yield return new object[] { new ProtoWireField(1, int64ArrayValue, wireType) };
            }
            foreach (var packedWireType in Enum.GetValues<WireType>().Where(x => x != WireType.Fixed64 && x != WireType.Varint && x != WireType.SignedVarint))
            {
                yield return new object[] { new ProtoWireField(1, int64ArrayValue, WireType.String, packedWireType) };
            }
            // Invalid wire type for String
            var stringValue = new ProtoWireValue("X");
            yield return new object[] { new ProtoWireField(1, stringValue, WireType.None) };
            yield return new object[] { new ProtoWireField(1, stringValue, WireType.Varint) };
            yield return new object[] { new ProtoWireField(1, stringValue, WireType.SignedVarint) };
            yield return new object[] { new ProtoWireField(1, stringValue, WireType.StartGroup) };
            yield return new object[] { new ProtoWireField(1, stringValue, WireType.EndGroup) };
            yield return new object[] { new ProtoWireField(1, stringValue, WireType.Fixed32) };
            yield return new object[] { new ProtoWireField(1, stringValue, WireType.Fixed64) };
            // Invalid wire type for Bytes
            var byteValue = new ProtoWireValue(new byte[] { 0, 1 });
            yield return new object[] { new ProtoWireField(1, byteValue, WireType.None) };
            yield return new object[] { new ProtoWireField(1, byteValue, WireType.Varint) };
            yield return new object[] { new ProtoWireField(1, byteValue, WireType.SignedVarint) };
            yield return new object[] { new ProtoWireField(1, byteValue, WireType.StartGroup) };
            yield return new object[] { new ProtoWireField(1, byteValue, WireType.EndGroup) };
            yield return new object[] { new ProtoWireField(1, byteValue, WireType.Fixed32) };
            yield return new object[] { new ProtoWireField(1, byteValue, WireType.Fixed64) };
            // Invalid wire type for Message
            var messageValue = new ProtoWireValue(new ProtoWireObject());
            yield return new object[] { new ProtoWireField(1, messageValue, WireType.None) };
            yield return new object[] { new ProtoWireField(1, messageValue, WireType.Varint) };
            yield return new object[] { new ProtoWireField(1, messageValue, WireType.SignedVarint) };
            yield return new object[] { new ProtoWireField(1, messageValue, WireType.EndGroup) };
            yield return new object[] { new ProtoWireField(1, messageValue, WireType.Fixed32) };
            yield return new object[] { new ProtoWireField(1, messageValue, WireType.Fixed64) };
            // Invalid nested field for Message
            var invalidMessageValue = new ProtoWireValue(new ProtoWireObject(new ProtoWireField(-1, 1)));
            yield return new object[] { new ProtoWireField(1, invalidMessageValue, WireType.String) };
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