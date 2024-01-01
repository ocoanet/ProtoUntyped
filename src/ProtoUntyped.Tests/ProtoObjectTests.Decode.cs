using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using ProtoUntyped.Tests.Messages;
using Xunit;
using Xunit.Sdk;

namespace ProtoUntyped.Tests;

public partial class ProtoObjectTests
{
    public class Decode
    {
        [Fact]
        public void ShouldParseSimpleMessage()
        {
            var message = ThreadLocalFixture.Create<SearchRequest>();

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.String, message.Query),
                new(2, WireType.Varint, (long)message.PageNumber),
                new(3, WireType.Varint, (long)message.ResultPerPage)
            ));
        }

        [Fact]
        public void ShouldParseMessageWithFloats()
        {
            var message = ThreadLocalFixture.Create<MessageWithFloats>();

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Fixed64, message.DoubleValue),
                new(2, WireType.Fixed32, message.SingleValue)
            ));
        }

        [Fact]
        public void ShouldParseMessageWithIntegers()
        {
            var message = ThreadLocalFixture.Create<MessageWithIntegers>();

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Varint, (long)message.Int16Value),
                new(2, WireType.Varint, (long)message.UInt16Value),
                new(3, WireType.Varint, (long)message.Int32Value),
                new(4, WireType.Varint, (long)message.UInt32Value),
                new(5, WireType.Varint, (long)message.Int64Value),
                new(6, WireType.Varint, (long)message.UInt64Value),
                new(7, WireType.Varint, (long)message.ByteValue),
                new(8, WireType.Varint, (long)message.SByteValue)
            ));
        }

        [Fact]
        public void ShouldParseMessageWithNestedTypes()
        {
            var message = ThreadLocalFixture.Create<MessageWithNestedTypes>();

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Varint, (long)message.Id),
                new(2, WireType.String, new ProtoObject(
                    new ProtoField(1, WireType.String, new ProtoObject(
                        new(1, WireType.Varint, (long)message.Nested1.Nested2.Value1),
                        new(2, WireType.String, message.Nested1.Nested2.Value2)
                    ))
                )),
                new(3, WireType.String, message.Key)
            ));
        }

        [Fact]
        public void ShouldParseMessageWithGroups()
        {
            var message = ThreadLocalFixture.Create<MessageWithGroups>();

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Varint, (long)message.Id),
                new(2, WireType.StartGroup, new ProtoObject(
                    new ProtoField(1, WireType.StartGroup, new ProtoObject(
                        new(1, WireType.Varint, (long)message.Nested1.Nested2.Value1),
                        new(2, WireType.String, message.Nested1.Nested2.Value2)
                    ))
                )),
                new(3, WireType.String, message.Key)
            ));

            Assert.Single(protoObject.GetFields(2)).IsGroup.ShouldEqual(true);

            var nestedObject2 = Assert.IsType<ProtoObject>(Assert.Single(protoObject.GetFields(2)).Value);
            Assert.Single(nestedObject2.GetFields(1)).IsGroup.ShouldEqual(true);
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
            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Varint, (long)message.Id),
                new RepeatedProtoField(2, WireType.Varint, new[] { 101L, 102L, 103L }),
                new RepeatedProtoField(3, WireType.String, new[]
                {
                    new ProtoObject(
                        new(1, WireType.Varint, 1001L),
                        new(2, WireType.String, "ABC1")
                    ),
                    new ProtoObject(
                        new(1, WireType.Varint, 1002L),
                        new(2, WireType.String, "ABC2")
                    ),
                })
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
            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Varint, (long)message.Id),
                new(2, WireType.String, message.Data)
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

            var protoObject = ProtoObject.Decode(bytes);

            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Varint, (long)message.Id),
                new(2, WireType.String, message.Data)
            ));
        }

        [Fact]
        public void ShouldParseMessageWithGuidAsEmbeddedMessage()
        {
            var message = ThreadLocalFixture.Create<MessageWithGuid>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);

            var protoMember = Assert.Single(protoObject.Fields);
            Assert.Equal(1, protoMember.FieldNumber);

            var embeddedObject = Assert.IsType<ProtoObject>(protoMember.Value);
            Assert.Equal(2, embeddedObject.Fields.Count);
        }

        [Fact]
        public void ShouldParseMessageWithGuidAsGuid()
        {
            var message = ThreadLocalFixture.Create<MessageWithGuid>();
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeGuid = true });

            protoObject.ShouldDeepEqual(new ProtoObject(
                new ProtoField(1, WireType.String, message.Guid)
            ));
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

            protoObject.ShouldDeepEqual(new ProtoObject(
                new ProtoField(1, WireType.String, message.Timestamp)
            ));
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

            protoObject.ShouldDeepEqual(new ProtoObject(
                new ProtoField(1, WireType.String, message.Duration)
            ));
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

            protoObject.ShouldDeepEqual(new ProtoObject(
                new ProtoField(1, WireType.String, message.Value)
            ));
        }
        
        [Fact]
        public void ShouldParseMessageWithRandomDecimal()
        {
            foreach (var _ in Enumerable.Range(0, 100))
            {
                var message = ThreadLocalFixture.Create<MessageWithDecimal>();
                var bytes = ProtoBufUtil.Serialize(message);

                var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDecimal = true });

                protoObject.ShouldDeepEqual(new ProtoObject(
                    new ProtoField(1, WireType.String, message.Value)
                ));
            }
        }

        [Theory]
        [MemberData(nameof(ShouldDecodeEmptyBytesUsingSpecifiedDecodingModeData))]
        public void ShouldDecodeEmptyBytesUsingSpecifiedDecodingMode(StringWireTypeDecodingMode decodingMode, ProtoField expectedField)
        {
            var message = new MessageWithByteArray { Data = Array.Empty<byte>() };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { PreferredStringDecodingModes = new[] { decodingMode } });

            protoObject.ShouldDeepEqual(new ProtoObject(expectedField));
        }

        public static IEnumerable<object[]> ShouldDecodeEmptyBytesUsingSpecifiedDecodingModeData
        {
            get
            {
                yield return [StringWireTypeDecodingMode.Bytes, new ProtoField(2, WireType.String, Array.Empty<byte>())];
                yield return [StringWireTypeDecodingMode.String, new ProtoField(2, WireType.String, "")];
                yield return [StringWireTypeDecodingMode.EmbeddedMessage, new ProtoField(2, WireType.String, ProtoObject.Empty)];
            }
        }

        [Fact]
        public void ShouldDecodeEmptyBytesUsingAllDecodingMode()
        {
            var message = new MessageWithByteArray { Data = Array.Empty<byte>() };
            var bytes = ProtoBufUtil.Serialize(message);

            foreach (var decodingMode in Enum.GetValues<StringWireTypeDecodingMode>())
            {
                ProtoObject.Decode(bytes, new ProtoDecodeOptions { PreferredStringDecodingModes = new[] { decodingMode } });
            }
        }

        [Fact]
        public void ShouldDecodeFixed32AsInt32()
        {
            var message = ThreadLocalFixture.Create<MessageWithFixedInt32>();

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { Fixed32DecodingMode = FixedWireTypeDecodingMode.Integer });
            
            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Varint, (long)message.Id),
                new(2, WireType.Fixed32, message.FixedValue),
                new(3, WireType.Varint, (long)message.Value),
                new(4, WireType.Fixed64, message.DoubleValue)
            ));
        }
        
        [Fact]
        public void ShouldDecodeFixed64AsInt64()
        {
            var message = ThreadLocalFixture.Create<MessageWithFixedInt64>();

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { Fixed64DecodingMode = FixedWireTypeDecodingMode.Integer });
            
            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Varint, (long)message.Id),
                new(2, WireType.Fixed64, message.FixedValue),
                new(3, WireType.Varint, message.Value),
                new(4, WireType.Fixed32, message.SingleValue)
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
            var protoObject = ProtoObject.Decode(bytes, decodeOptions);

            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Varint, 123L),
                new(2, WireType.String, new[] { 101, 102, 103 }, WireType.Fixed32)
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
            var protoObject = ProtoObject.Decode(bytes, decodeOptions);

            protoObject.ShouldDeepEqual(new ProtoObject(
                new(1, WireType.Varint, 123L),
                new(2, WireType.String, [101L, 102L, 103L], WireType.Fixed64)
            ));
        }

        [Fact]
        public void ShouldDecodeSmallAsciiStringAsString()
        {
            // REM: using theory with so many inputs would make the IDE hang.
            
            foreach (var s in GetSmallAsciiStrings())
            {
                var message = new MessageWithString { Data = s };

                var bytes = ProtoBufUtil.Serialize(message);
                var protoObject = ProtoObject.Decode(bytes);

                var expectedProtoObject = new ProtoObject(new ProtoField(2, WireType.String, s));
                if (!protoObject.DeepEquals(expectedProtoObject))
                    throw new XunitException($"Invalid object for string [{s}]");
            }
        }

        private static IEnumerable<string> GetSmallAsciiStrings()
        {
            const string asciiChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            foreach (var c1 in asciiChars)
            {
                yield return c1.ToString();
                
                foreach (var c2 in asciiChars)
                {
                    yield return c1.ToString() + c2;
                    
#if !DEBUG
                    foreach (var c3 in asciiChars)
                    {
                        yield return c1.ToString() + c2 + c3;
                    }
#endif
                }
            }
        }
    }
}
