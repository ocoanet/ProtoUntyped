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
        var nestedObject = new ProtoObject
        {
            Fields =
            {
                new ProtoField(2, 1, WireType.Fixed32),
                new ProtoField(1, 1, WireType.Fixed32),
            },
        };
        
        var protoObject = new ProtoObject
        {
            Fields =
            {
                new ProtoField(7, "X", WireType.String),
                new ProtoField(6, "X", WireType.String),
                new ProtoField(4, 1, WireType.Fixed32),
                new ProtoField(5, nestedObject),
            },
        };

        protoObject.SortFields();

        var fieldNumbers = protoObject.Fields.Select(x => x.FieldNumber).ToList();
        fieldNumbers.ShouldDeepEqual(new List<int> { 4, 5, 6, 7 });
        
        var nestedFieldNumbers = nestedObject.Fields.Select(x => x.FieldNumber).ToList();
        nestedFieldNumbers.ShouldDeepEqual(new List<int> { 2, 1 });
    }
    
    [Fact]
    public void ShouldSortMembersRecursively()
    {
        var nestedObject1 = new ProtoObject
        {
            Fields =
            {
                new ProtoField(2, 1, WireType.Fixed32),
                new ProtoField(1, 1, WireType.Fixed32),
            },
        };
        
        var nestedObject2 = new ProtoObject
        {
            Fields =
            {
                new ProtoField(9, 1, WireType.Fixed32),
                new ProtoField(8, 1, WireType.Fixed32),
            },
        };

        var protoObject = new ProtoObject
        {
            Fields =
            {
                new ProtoField(7, "X", WireType.String),
                new ProtoField(6, "X", WireType.String),
                new ProtoField(4, 1, WireType.Fixed32),
                new ProtoField(5, nestedObject1),
                new RepeatedProtoField(9, new ProtoValue[] { new ProtoValue(nestedObject2, WireType.String) }),
            },
        };

        protoObject.SortFields(true);

        var fieldNumbers = protoObject.Fields.Select(x => x.FieldNumber).ToList();
        fieldNumbers.ShouldDeepEqual(new List<int> { 4, 5, 6, 7, 9 });
        
        var nestedFieldNumbers1 = nestedObject1.Fields.Select(x => x.FieldNumber).ToList();
        nestedFieldNumbers1.ShouldDeepEqual(new List<int> { 1, 2 });
        
        var nestedFieldNumbers2 = nestedObject2.Fields.Select(x => x.FieldNumber).ToList();
        nestedFieldNumbers2.ShouldDeepEqual(new List<int> { 8, 9 });
    }
}