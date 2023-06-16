using ProtoBuf;
using Xunit;

namespace ProtoUntyped.Tests;

public class ProtoFieldTests
{
    [Fact]
    public void ShouldGetStringFromField()
    {
        var field = new ProtoField(5, "Text", WireType.String);
        
        field.ToString().ShouldEqual(
            """
            - 5 = "Text"

            """);
    }
}
