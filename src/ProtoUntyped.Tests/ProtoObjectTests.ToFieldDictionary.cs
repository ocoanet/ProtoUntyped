using System.Collections.Generic;
using ProtoUntyped.Tests.Messages;
using Xunit;

namespace ProtoUntyped.Tests;

partial class ProtoObjectTests
{
    public class ToFieldDictionary
    {
        [Fact]
        public void ShouldGetFieldDictionaryWithSimpleMessage()
        {
            var message = new SearchRequest
            {
                Query = "/users",
                PageNumber = 2,
                ResultPerPage = 100,
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes);
            var dictionary = protoObject.ToFieldDictionary();

            dictionary.ShouldDeepEqual(new()
            {
                [1] = "/users",
                [2] = 2L,
                [3] = 100L,
            });
        }

        [Fact]
        public void ShouldGetFieldDictionaryWithArrays()
        {
            var message = new MessageWithArrays
            {
                Id = 123,
                Int32Array = new[] { 1, 2, 3 },
                MessageArray = new[]
                {
                    new MessageWithArrays.Nested { Id = 1001, Key = "1" },
                    new MessageWithArrays.Nested { Id = 1002, Key = "2" },
                },
            };

            var bytes = ProtoBufUtil.Serialize(message);
            var protoObject = ProtoObject.Decode(bytes);
            var dictionary = protoObject.ToFieldDictionary();

            dictionary.ShouldDeepEqual(new()
            {
                [1] = 123L,
                [2] = new[] { 1L, 2L, 3L },
                [3] = new object[]
                {
                    new Dictionary<int, object> { [1] = 1001L, [2] = "1" },
                    new Dictionary<int, object> { [1] = 1002L, [2] = "2" },
                },
            });
        }
    }
}