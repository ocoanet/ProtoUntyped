using System;
using System.Globalization;
using ProtoBuf;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests;

partial class ProtoObjectTests
{
    public class ToString
    {
        [Fact]
        public void ShouldGetStringWithSimpleType()
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
                message {
                - 1 = "/users"
                - 2 = 5
                - 3 = 40
                }

                """;

            protoObject.ToString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetStringWithGuid()
        {
            var message = new MessageWithGuid
            {
                Guid = Guid.NewGuid(),
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeGuid = true });

            var expectedText =
                $$"""
                message {
                - 1 = "{{message.Guid}}"
                }

                """;

            protoObject.ToString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetStringWithNestedTypes()
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
                message {
                - 1 = 42
                - 2 = message {
                    - 1 = message {
                        - 1 = 333
                        - 2 = "ABC"
                        }
                    }
                - 3 = "K"
                }

                """;

            protoObject.ToString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetStringWithByteArray()
        {
            var data = ThreadLocalFixture.CreateMany<byte>(20);
            var message = new MessageWithByteArray { Id = 123, Data = data };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);
            var expectedText =
                $$"""
                message {
                - 1 = {{message.Id}}
                - 2 = {{Convert.ToBase64String(data)}}
                }
                
                """;
                
            protoObject.ToString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetStringWithInt32Array()
        {
            var message = new MessageWithArrays { Id = 100, Int32Array = [1, 2, 3] };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes);

            var expectedText =
                """
                message {
                - 1 = 100
                - 2 = array [ 1 2 3 ]
                }
                
                """;
                
            protoObject.ToString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetStringWithDecimalArray()
        {
            var message = new MessageWithDecimalArray { Values = new[] { 1.1m, 2.2m, 3.3m } };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDecimal = true });
            var expectedText =
                """
                message {
                - 1 = array [ 1.1 2.2 3.3 ]
                }
                
                """;
                
            protoObject.ToString().ShouldEqual(expectedText);
        }

        [Theory]
        [InlineData("2021-10-23 15:29:53.123")]
        [InlineData("2021-10-23 15:29:53")]
        [InlineData("2021-10-23 15:29:00")]
        [InlineData("2021-10-23 15:00:00")]
        [InlineData("2021-10-23")]
        [InlineData("2001-06-06")]
        [InlineData("2030-06-06")]
        public void ShouldGetStringWithDateTime(string dateTime)
        {
            var message = new MessageWithDateTime { Timestamp = DateTime.Parse(dateTime, CultureInfo.InvariantCulture) };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDateTime = true, DecodeTimeSpan = true });

            var expectedText =
                $$"""
                  message {
                  - 1 = "{{dateTime}}"
                  }

                  """;

            protoObject.ToString().ShouldEqual(expectedText);
        }

        [Theory]
        [InlineData("15:29:53.123")]
        [InlineData("00:00:02")]
        [InlineData("15:29:53")]
        [InlineData("15:35:00")]
        [InlineData("15:00:00")]
        [InlineData("5.15:16:17")]
        public void ShouldGetStringWithTimeSpan(string timeSpan)
        {
            var message = new MessageWithTimeSpan { Duration = TimeSpan.Parse(timeSpan, CultureInfo.InvariantCulture) };
            var bytes = ProtoBufUtil.Serialize(message);

            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDateTime = true, DecodeTimeSpan = true });

            var expectedText =
                $$"""
                message {
                - 1 = "{{timeSpan}}"
                }

                """;

            protoObject.ToString().ShouldEqual(expectedText);
        }

        [Fact]
        public void ShouldGetStringWithDateTimeArray()
        {
            var message = new MessageWithDateTimeArray
            {
                Id = 123,
                Timestamps = new()
                {
                    new DateTime(2024, 01, 01, 14, 00, 00),
                    new DateTime(2024, 01, 31, 09, 30, 00),
                }
            };
            
            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDateTime = true, DecodeTimeSpan = true });

            var expectedText =
                """
                message {
                - 1 = 123
                - 2 = array [
                    "2024-01-01 14:00:00"
                    "2024-01-31 09:30:00"
                    ]
                }

                """;

            protoObject.ToString().ShouldEqual(expectedText);
        }
        
        [Fact]
        public void ShouldGetStringForAllMessages()
        {
            var formatter = new StrictProtoFormatter();
            var messages = TestData.CreateTestMessages();

            foreach (var message in messages)
            {
                var bytes = ProtoBufUtil.Serialize(message);
                var protoObject = ProtoObject.Decode(bytes, TestData.GetDecodeOptions(message));
                
                protoObject.ToString(formatter);
            }
        }
    }
}
