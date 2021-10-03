using System.Linq;
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
                    new ProtoField(2, new ProtoObject { Members =
                    {
                        new ProtoField(1, (long)message.Nested.Value1, WireType.Varint),
                        new ProtoField(2, message.Nested.Value2, WireType.String),
                    }}),
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
        
        [Fact]
        public void ShouldParseMessageWithBytes()
        {
            var message = new MessageWithBytes
            {
                Id = 42,
                Data = new byte[] { 0, 1, 200 },
            };
            
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { StringDecoder = ProtoStringDecoder.AsciiOnly });

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

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeGuids = true });
            
            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Members =
                {
                    new ProtoField(1, message.Guid, WireType.String),   
                }
            });
        }
    }
}
