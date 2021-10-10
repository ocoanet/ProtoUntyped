using System;
using System.Linq;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests
{
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
                "[message]",
                "- 1 = \"/users\"",
                "- 2 = 5",
                "- 3 = 40",
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
                "[message]",
                "- 1 = \"" + message.Guid + "\"",
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
                "[message]",
                "- 1 = 42",
                "- 2 = [message]",
                "    - 1 = [message]",
                "        - 1 = 333",
            });

            protoObject.ToString().ShouldEqual(expectedText);
        }

        private static string MergeLines(string[] lines) => string.Concat(lines.Select(x => x + Environment.NewLine));
    }
}
