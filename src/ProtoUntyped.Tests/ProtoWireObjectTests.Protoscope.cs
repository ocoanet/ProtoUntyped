using System;
using ProtoBuf;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests;

public partial class ProtoWireObjectTests
{
    public class Protoscope
    {
        [Fact]
        public void ShouldGetProtoscopeStringWithSimpleType()
        {
            var message = new SearchRequest
            {
                Query = "/users",
                PageNumber = 5,
                ResultPerPage = 40,
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var obj = ProtoWireObject.Decode(bytes);

            var expectedText =
                """
                1: {"/users"}
                2: 5
                3: 40

                """;

            obj.ToProtoscopeString().ShouldEqual(expectedText);
        }
        
        [Fact]
        public void ShouldGetProtoscopeStringWithNestedTypes()
        {
            var message = new MessageWithNestedTypes
            {
                Id = 42,
                Nested1 = new()
                {
                    Nested2 = new() { Value1 = 333, Value2 = "ABC" },
                },
                Key = "K",
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var obj = ProtoWireObject.Decode(bytes);

            var expectedText =
                """
                1: 42
                2: {
                  1: {
                    1: 333
                    2: {"ABC"}
                  }
                }
                3: {"K"}

                """;

            obj.ToProtoscopeString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetProtoscopeStringWithByteArray()
        {
            var data = ThreadLocalFixture.CreateMany<byte>(20);
            var message = new MessageWithByteArray { Id = 123, Data = data };
            var bytes = ProtoBufUtil.Serialize(message);

            var obj = ProtoWireObject.Decode(bytes);
            
            var expectedText =
                $$"""
                1: {{message.Id}}
                2: {`{{Convert.ToHexString(data).ToLower()}}`}

                """;

            obj.ToProtoscopeString().ShouldEqual(expectedText);
        }
        
        [Fact]
        public void ShouldGetProtoscopeStringForEmptyGroup()
        {
            var obj = new ProtoWireObject(
                new ProtoWireField(1, WireType.StartGroup, new ProtoWireObject())
            );

            var expectedText =
                """
                1: !{}

                """;

            obj.ToProtoscopeString().ShouldEqual(expectedText);
        }
        
        [Fact]
        public void ShouldGetProtoscopeStringWithPackedVarintArray()
        {
            var message = new MessageWithPackedVarintArray
            {
                Id = 123,
                Int32Array = [101, 102],
                Int64Array = [1001, 1002],
            };
            
            var bytes = ProtoBufUtil.Serialize(message);
            var obj = ProtoWireObject.Decode(bytes, new ProtoDecodeOptions { PreferredStringDecodingModes = [StringWireTypeDecodingMode.PackedVarint] });
            
            var expectedText =
                """
                  1: 123
                  2: { 101 102 }
                  3: { 1001 1002 }

                  """;

            obj.ToProtoscopeString().ShouldEqual(expectedText);
        }
        
        [Fact]
        public void ShouldGetProtoscopeStringWithPackedFixed32Array()
        {
            var message = new MessageWithPackedFixed32Array
            {
                Id = 123,
                Int32Array = [101, 102],
            };
            
            var bytes = ProtoBufUtil.Serialize(message);
            var obj = ProtoWireObject.Decode(bytes, new ProtoDecodeOptions { PreferredStringDecodingModes = [StringWireTypeDecodingMode.PackedFixed32] });
            
            var expectedText =
                """
                1: 123
                2: { 0x65i32 0x66i32 }

                """;

            obj.ToProtoscopeString().ShouldEqual(expectedText);
        }
        
        [Fact]
        public void ShouldGetProtoscopeStringWithPackedFixed64Array()
        {
            var message = new MessageWithPackedFixed64Array
            {
                Id = 123,
                Int64Array = [101, 102],
            };
            
            var bytes = ProtoBufUtil.Serialize(message);
            var obj = ProtoWireObject.Decode(bytes, new ProtoDecodeOptions { PreferredStringDecodingModes = [StringWireTypeDecodingMode.PackedFixed64] });
            
            var expectedText =
                """
                1: 123
                2: { 0x65i64 0x66i64 }

                """;

            obj.ToProtoscopeString().ShouldEqual(expectedText);
        }
    }
}