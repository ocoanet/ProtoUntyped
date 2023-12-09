using ProtoBuf;
using Xunit;

namespace ProtoUntyped.Tests;

public class RepeatedProtoFieldTests
{
    [Fact]
    public void ShouldGetStringFromArray()
    {
        var protoArray = new RepeatedProtoField(5, WireType.String, new[] { "Text1", "Text2" });

        var expectedText = """
            - 5 = array [
                - 0 = "Text1"
                - 1 = "Text2"
                ]

            """;
        
        protoArray.ToString().ShouldEqual(expectedText);
    }
}
