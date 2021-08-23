using System.Collections.Generic;
using System.Linq;
using DeepEqual.Syntax;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests
{
    public class ProtoObjectTests
    {
        [Fact]
        public void ShouldParseSimpleMessage()
        {
            var message = ThreadLocalFixture.Create<SearchRequest>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Parse(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Fields =
                {
                    new(1, message.Query),
                    new(2, (long)message.PageNumber),
                    new(3, (long)message.ResultPerPage),
                }
            });
        }
        
        [Fact]
        public void ShouldParseMessageWithFloats()
        {
            var message = ThreadLocalFixture.Create<MessageWithFloats>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Parse(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Fields =
                {
                    new(1, message.DoubleValue),
                    new(2, message.SingleValue),
                }
            });
        }
        
        [Fact]
        public void ShouldParseMessageWithIntegers()
        {
            var message = ThreadLocalFixture.Create<MessageWithIntegers>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Parse(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Fields =
                {
                    new(1, (long)message.Int16Value),
                    new(2, (long)message.UInt16Value),
                    new(3, (long)message.Int32Value),
                    new(4, (long)message.UInt32Value),
                    new(5, (long)message.Int64Value),
                    new(6, (long)message.UInt64Value),
                    new(7, (long)message.ByteValue),
                    new(8, (long)message.SByteValue),
                }
            });
        }
        
        [Fact]
        public void ShouldParseMessageWithNestedType()
        {
            var message = ThreadLocalFixture.Create<MessageWithNestedType>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Parse(bytes);
            
            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Fields =
                {
                    new(1, (long)message.Id),
                    new(2, new ProtoObject { Fields =
                    {
                        new ProtoField(1, (long)message.Nested.Value1),
                        new ProtoField(2, message.Nested.Value2),
                    }}),
                    new(3, message.Key),    
                }
            });
        }
        
        [Fact]
        public void ShouldParseMessageWithArrays()
        {
            var message = ThreadLocalFixture.Create<MessageWithArrays>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Parse(bytes);
            
            protoObject.ShouldDeepEqual(new ProtoObject
            {
                Fields =
                {
                    new(1, (long)message.Id),
                    new(2, new List<object>(message.Int32Array.Select(x => (object)(long)x))),
                    new(3, new List<object>(message.MessageArray.Select(x => (object)ToProtoObject(x)))),    
                }
            });

            static ProtoObject ToProtoObject(MessageWithArrays.Nested nested)
            {
                return new ProtoObject
                {
                    Fields =
                    {
                        new ProtoField(1, (long)nested.Id),
                        new ProtoField(2, nested.Key),
                    }
                };
            }
        }
    }
}
