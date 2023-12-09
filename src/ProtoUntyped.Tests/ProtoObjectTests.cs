using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Xunit;

namespace ProtoUntyped.Tests;

public partial class ProtoObjectTests
{
    [Fact]
    public void ShouldSortMembers()
    {
        var protoObject = new ProtoObject(new List<ProtoField>
        {
            new(7, WireType.String, "X"),
            new(6, WireType.String, "X"),
            new(4, WireType.Fixed32, 1),
            new(5, WireType.String, new ProtoObject(new List<ProtoField>
            {
                new(2, WireType.Fixed32, 1),
                new(1, WireType.Fixed32, 1),
            })),
        });

        protoObject.SortFields();

        var fieldNumbers = protoObject.Fields.Select(x => x.FieldNumber).ToList();
        fieldNumbers.ShouldDeepEqual(new List<int> { 4, 5, 6, 7 });

        var nestedObject = Assert.IsType<ProtoObject>(Assert.Single(protoObject.GetFields(5)).Value);
        var nestedFieldNumbers = nestedObject.Fields.Select(x => x.FieldNumber).ToList();
        nestedFieldNumbers.ShouldDeepEqual(new List<int> { 2, 1 });
    }
    
    [Fact]
    public void ShouldSortMembersRecursively()
    {
        var protoObject = new ProtoObject(new List<ProtoField>
        {
            new(7, WireType.String, "X"),
            new(6, WireType.String, "X"),
            new(4, WireType.Fixed32, 1),
            new(9, WireType.String, new ProtoObject(new List<ProtoField>
            {
                new(2, WireType.Fixed32, 1),
                new(1, WireType.Fixed32, 1),
            })),
            new(5, WireType.String, new ProtoObject(new List<ProtoField>
            {
                new(9, WireType.Fixed32, 1),
                new(8, WireType.Fixed32, 1),
            })),
        });

        protoObject.SortFields(true);

        var fieldNumbers = protoObject.Fields.Select(x => x.FieldNumber).ToList();
        fieldNumbers.ShouldDeepEqual(new List<int> { 4, 5, 6, 7, 9 });
        
        var nestedObject1 = Assert.IsType<ProtoObject>(Assert.Single(protoObject.GetFields(5)).Value);
        var nestedFieldNumbers1 = nestedObject1.Fields.Select(x => x.FieldNumber).ToList();
        nestedFieldNumbers1.ShouldDeepEqual(new List<int> { 8, 9 });
        
        var nestedObject2 = Assert.IsType<ProtoObject>(Assert.Single(protoObject.GetFields(9)).Value);
        var nestedFieldNumbers2 = nestedObject2.Fields.Select(x => x.FieldNumber).ToList();
        nestedFieldNumbers2.ShouldDeepEqual(new List<int> { 1, 2 });
    }
}