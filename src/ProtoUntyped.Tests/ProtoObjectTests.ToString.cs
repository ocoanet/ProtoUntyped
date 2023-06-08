using System;
using System.Globalization;
using System.Linq;
using ProtoBuf;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests;

partial class ProtoObjectTests
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

        var expectedText = MergeLines(new[]
        {
            "message {",
            "- 1 = \"/users\"",
            "- 2 = 5",
            "- 3 = 40",
            "}",
        });

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

        var expectedText = MergeLines(new[]
        {
            "message {",
            "- 1 = \"" + message.Guid + "\"",
            "}",
        });

        protoObject.ToString().ShouldEqual(expectedText);
    }
        
    [Fact]
    public void ShouldGetStringWithNestedNestedTypes()
    {
        var message = new MessageWithMultipleNestedTypes
        {
            Id = 42,
            Nested1 = new()
            {
                Nested2 = new() { Value = 333 },
            }
        };

        var bytes = ProtoBufUtil.Serialize(message);
        var protoObject = ProtoObject.Decode(bytes);

        var expectedText = MergeLines(new[]
        {
            "message {",
            "- 1 = 42",
            "- 2 = message {",
            "    - 1 = message {",
            "        - 1 = 333",
            "        }",
            "    }",
            "}",
        });

        protoObject.ToString().ShouldEqual(expectedText);
    }

    [Fact]
    public void ShouldGetStringWithByteArray()
    {
        var data = ThreadLocalFixture.CreateMany<byte>(20);
        var message = new MessageWithByteArray { Data = data };
        var bytes = ProtoBufUtil.Serialize(message);
            
        var protoObject = ProtoObject.Decode(bytes);
        var protoText = protoObject.ToString();
            
        protoText.ShouldContain(Convert.ToBase64String(data));
    }

    [Fact]
    public void ShouldGetStringWithInt32Array()
    {
        var message = new MessageWithArrays { Int32Array = new[] { 1, 2, 3 } };
        var bytes = ProtoBufUtil.Serialize(message);
            
        var protoObject = ProtoObject.Decode(bytes);
        var protoText = protoObject.ToString();
            
        protoText.ShouldContain("array [ 1 2 3 ]");
    }

    [Fact]
    public void ShouldGetStringWithDecimalArray()
    {
        var message = new MessageWithDecimalArray { Values = new[] { 1.1m, 2.2m, 3.3m } };
        var bytes = ProtoBufUtil.Serialize(message);

        var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDecimal = true });
        var protoText = protoObject.ToString();
            
        protoText.ShouldContain("array [ 1.1 2.2 3.3 ]");
    }

    [Theory]
    [InlineData("2021-10-23 15:29:53.123" )]
    [InlineData("2021-10-23 15:29:53" )]
    [InlineData("2021-10-23 15:29:00" )]
    [InlineData("2021-10-23 15:00:00" )]
    [InlineData("2021-10-23" )]
    [InlineData("2001-06-06" )]
    [InlineData("2030-06-06" )]
    public void ShouldGetStringWithDateTime(string dateTime)
    {
        var message = new MessageWithDateTime { Timestamp = DateTime.Parse(dateTime, CultureInfo.InvariantCulture) };
        var bytes = ProtoBufUtil.Serialize(message);
        
        var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDateTime = true, DecodeTimeSpan = true });
            
        var expectedText = MergeLines(new[]
        {
            "message {",
            "- 1 = \"" + dateTime + "\"",
            "}",
        });

        protoObject.ToString().ShouldEqual(expectedText);
            
        protoObject.ShouldDeepEqual(new ProtoObject
        {
            Members =
            {
                new ProtoField(1, message.Timestamp, WireType.String),   
            }
        });
    }
        
    [Theory]
    [InlineData("15:29:53.123" )]
    [InlineData("00:00:02" )]
    [InlineData("15:29:53" )]
    [InlineData("15:35:00" )]
    [InlineData("15:00:00" )]
    [InlineData("5.15:16:17" )]
    public void ShouldGetStringWithTimeSpan(string timeSpan)
    {
        var message = new MessageWithTimeSpan { Duration = TimeSpan.Parse(timeSpan, CultureInfo.InvariantCulture) };
        var bytes = ProtoBufUtil.Serialize(message);
        
        var protoObject = ProtoObject.Decode(bytes, new ProtoDecodeOptions { DecodeDateTime = true, DecodeTimeSpan = true });
            
        var expectedText = MergeLines(new[]
        {
            "message {",
            "- 1 = \"" + timeSpan + "\"",
            "}",
        });

        protoObject.ToString().ShouldEqual(expectedText);
    }

    private static string MergeLines(string[] lines) => string.Concat(lines.Select(x => x + Environment.NewLine));
}