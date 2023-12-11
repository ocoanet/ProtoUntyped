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
    }
}