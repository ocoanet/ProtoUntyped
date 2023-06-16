using ProtoBuf;
using Xunit;

namespace ProtoUntyped.Tests;

public class ProtoArrayTests
{
    [Fact]
    public void ShouldGetStringFromArray()
    {
        var protoArray = new ProtoArray(5, new[] { new ProtoValue("Text1", WireType.String), new ProtoValue("Text2", WireType.String) });
        
        protoArray.ToString().ShouldEqual(
            """
            - 5 = array [
                - 0 = "Text1"
                - 1 = "Text2"
                ]

            """);
    }
}
