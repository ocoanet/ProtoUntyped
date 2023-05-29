using System;
using System.Linq;
using System.Text;
using ProtoBuf;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests
{
    public partial class ProtoObjectTests
    {
        [Fact]
        public void ShouldParseSimpleMessage()
        {
            var message = ThreadLocalFixture.Create<SearchRequest>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, message.Query, WireType.String),
                    new ProtoField(2, (long)message.PageNumber, WireType.Varint),
                    new ProtoField(3, (long)message.ResultPerPage, WireType.Varint),
                }
            });
        }

        [Fact]
        public void ShouldParseMessageWithFloats()
        {
            var message = ThreadLocalFixture.Create<MessageWithFloats>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, message.DoubleValue, WireType.Fixed64),
                    new ProtoField(2, message.SingleValue, WireType.Fixed32),
                }
            });
        }

        [Fact]
        public void ShouldParseMessageWithIntegers()
        {
            var message = ThreadLocalFixture.Create<MessageWithIntegers>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, (long)message.Int16Value, WireType.Varint),
                    new ProtoField(2, (long)message.UInt16Value, WireType.Varint),
                    new ProtoField(3, (long)message.Int32Value, WireType.Varint),
                    new ProtoField(4, (long)message.UInt32Value, WireType.Varint),
                    new ProtoField(5, (long)message.Int64Value, WireType.Varint),
                    new ProtoField(6, (long)message.UInt64Value, WireType.Varint),
                    new ProtoField(7, (long)message.ByteValue, WireType.Varint),
                    new ProtoField(8, (long)message.SByteValue, WireType.Varint),
                }
            });
        }

        [Fact]
        public void ShouldParseMessageWithNestedType()
        {
            var message = ThreadLocalFixture.Create<MessageWithNestedType>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, (long)message.Id, WireType.Varint),
                    new ProtoField(2,
                                   new ProtoObject
                                   {
                                       Members =
                                       {
                                           new ProtoField(1, (long)message.Nested.Value1, WireType.Varint),
                                           new ProtoField(2, message.Nested.Value2, WireType.String),
                                       }
                                   }),
                    new ProtoField(3, message.Key, WireType.String),
                }
            });
        }

        [Fact]
        public void ShouldParseMessageWithArrays()
        {
            var message = ThreadLocalFixture.Create<MessageWithArrays>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, (long)message.Id, WireType.Varint),
                    new ProtoArray(2, message.Int32Array.Select(x => new ProtoValue((long)x, WireType.Varint)).ToArray()),
                    new ProtoArray(3, message.MessageArray.Select(x => new ProtoValue(ToProtoObject(x), WireType.String)).ToArray()),
                }
            });

            static ProtoObject ToProtoObject(MessageWithArrays.Nested nested)
            {
                return new ProtoObject
                {
                    Members =
                    {
                        new ProtoField(1, (long)nested.Id, WireType.Varint),
                        new ProtoField(2, nested.Key, WireType.String),
                    }
                };
            }
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

            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, (long)message.Id, WireType.Varint),
                    new ProtoField(2, message.Data, WireType.String),
                }
            });
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

            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, (long)message.Id, WireType.Varint),
                    new ProtoField(2, message.Data, WireType.String),
                }
            });
        }

        [Fact]
        public void ShouldParseMessageWithGuidAsEmbeddedMessage()
        {
            var message = ThreadLocalFixture.Create<MessageWithGuid>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);

            var protoMember = Assert.Single(protoObject.Members);
            Assert.Equal(1, protoMember.FieldNumber);

            var embeddedObject = Assert.IsType<ProtoObject>(protoMember.Value);
            Assert.Equal(2, embeddedObject.Members.Count);
        }

        [Fact]
        public void ShouldParseMessageWithGuidAsGuid()
        {
            var message = ThreadLocalFixture.Create<MessageWithGuid>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeGuid = true });

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, message.Guid, WireType.String),
                }
            });
        }

        [Theory]
        [InlineData("2021-10-23 15:29:53.1234567")]
        [InlineData("2021-10-23 15:29:53.123")]
        [InlineData("2021-10-23 15:29:53")]
        [InlineData("2021-10-23 15:29")]
        [InlineData("2021-10-23 15:00")]
        [InlineData("2021-10-23")]
        [InlineData("2001-06-06")]
        [InlineData("2030-06-06")]
        public void ShouldParseMessageWithDateTime(DateTime value)
        {
            var message = new MessageWithDateTime { Timestamp = value };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDateTime = true, DecodeTimeSpan = true });

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, message.Timestamp, WireType.String),
                }
            });
        }

        [Theory]
        [InlineData("0001-01-01 15:29:53.1234567")]
        [InlineData("0001-01-01 15:29:53.123")]
        [InlineData("0001-01-01 00:00:02")]
        [InlineData("0001-01-01 15:29:53")]
        [InlineData("0001-01-01 15:35:00")]
        [InlineData("0001-01-01 15:00:00")]
        public void ShouldParseMessageWithTimeSpan(DateTime value)
        {
            var message = new MessageWithTimeSpan { Duration = value.TimeOfDay };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDateTime = true, DecodeTimeSpan = true });

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, message.Duration, WireType.String),
                }
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(0.123)]
        [InlineData(10000000)]
        [InlineData(89765464.546748767643)]
        public void ShouldParseMessageWithDecimal(decimal value)
        {
            var message = new MessageWithDecimal { Value = value };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDecimal = true });

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, message.Value, WireType.String),
                }
            });
        }
    }
}
