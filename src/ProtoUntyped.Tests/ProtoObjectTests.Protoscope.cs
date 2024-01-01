using System;
using System.Globalization;
using System.Linq;
using ProtoBuf;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests;

partial class ProtoObjectTests
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
            var protoObject = ProtoObject.Decode(bytes);

            var expectedText =
                """
                1: {"/users"}
                2: 5
                3: 40

                """;

            protoObject.ToProtoscopeString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetProtoscopeStringWithGuid()
        {
            var message = new MessageWithGuid
            {
                Guid = Guid.NewGuid(),
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeGuid = true });

            var expectedText =
                $$"""
                1: {"{{message.Guid}}"}

                """;

            protoObject.ToProtoscopeString().ShouldEqual(expectedText);
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
            var protoObject = ProtoObject.Decode(bytes);

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

            protoObject.ToProtoscopeString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetProtoscopeStringWithByteArray()
        {
            var data = ThreadLocalFixture.CreateMany<byte>(20);
            var message = new MessageWithByteArray { Id = 123, Data = data };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);
            
            var expectedText =
                $$"""
                  1: {{message.Id}}
                  2: {`{{Convert.ToHexString(data).ToLower()}}`}

                  """;
            
            protoObject.ToProtoscopeString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetProtoscopeStringWithInt32Array()
        {
            var message = new MessageWithArrays { Id = 100, Int32Array = new[] { 1, 2, 3 } };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);
            var expectedText =
                """
                1: 100
                2: { 1 2 3 }

                """;
                
            protoObject.ToProtoscopeString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetProtoscopeStringWithDecimalArray()
        {
            var message = new MessageWithDecimalArray { Values = new[] { 1.1m, 2.2m, 3.3m } };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDecimal = true });
            var expectedText =
                """
                1: { 1.1m 2.2m 3.3m }

                """;
                
            protoObject.ToProtoscopeString().ShouldEqual(expectedText);
        }

        [Theory]
        [InlineData("2021-10-23 15:29:53.123")]
        [InlineData("2021-10-23 15:29:53")]
        [InlineData("2021-10-23 15:29:00")]
        [InlineData("2021-10-23 15:00:00")]
        [InlineData("2021-10-23")]
        [InlineData("2001-06-06")]
        [InlineData("2030-06-06")]
        public void ShouldGetProtoscopeStringWithDateTime(string dateTime)
        {
            var message = new MessageWithDateTime { Timestamp = DateTime.Parse(dateTime, CultureInfo.InvariantCulture) };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDateTime = true, DecodeTimeSpan = true });

            var expectedText =
                $$"""
                1: "{{dateTime}}"

                """;

            protoObject.ToProtoscopeString().ShouldEqual(expectedText);
        }

        [Theory]
        [InlineData("15:29:53.123")]
        [InlineData("00:00:02")]
        [InlineData("15:29:53")]
        [InlineData("15:35:00")]
        [InlineData("15:00:00")]
        [InlineData("5.15:16:17")]
        public void ShouldGetProtoscopeStringWithTimeSpan(string timeSpan)
        {
            var message = new MessageWithTimeSpan { Duration = TimeSpan.Parse(timeSpan, CultureInfo.InvariantCulture) };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDateTime = true, DecodeTimeSpan = true });

            var expectedText =
                $$"""
                1: "{{timeSpan}}"

                """;

            protoObject.ToProtoscopeString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetProtoscopeStringForAllMessages()
        {
            var formatter = new StrictProtoscopeFormatter();
            var messages = TestData.CreateTestMessages();

            foreach (var message in messages)
            {
                var bytes = ProtoBufUtil.Serialize(message);
                var protoObject = ProtoObject.Decode(bytes, TestData.GetDecodeOptions(message));
                
                protoObject.ToProtoscopeString(formatter);
            }
        }
    }
}
