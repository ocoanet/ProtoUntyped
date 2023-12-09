using ProtoBuf;
using Xunit;

namespace ProtoUntyped.Tests;

public class ProtoFieldTests
{
    [Fact]
    public void ShouldGetStringFromField()
    {
        var field = new ProtoField(5, WireType.String, "Text");

        var expectedText = """
            - 5 = "Text"

            """;
        
        field.ToString().ShouldEqual(expectedText);
    }
}
