using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProtoBuf;

namespace ProtoUntyped;

[DebuggerTypeProxy(typeof(RepeatedProtoFieldDebugView))]
[DebuggerDisplay("FieldNumber = {FieldNumber}, Length = {Elements.Length}")]
public class RepeatedProtoField : ProtoField
{
    public RepeatedProtoField(int fieldNumber, ProtoValue[] elements)
        : base(fieldNumber, ComputeValue(elements), ComputeWireType(elements))
    {
        Elements = elements;
    }

    public ProtoValue[] Elements { get; }

    public override string ToString()
    {
        return ToString(ProtoFormatter.Default);
    }

    public string ToString(ProtoFormatter formatter)
    {
        return formatter.BuildString(this);
    }

    public override IEnumerable<ProtoValue> GetProtoValues()
    {
        return Elements;
    }

    private static object ComputeValue(ProtoValue[] items)
    {
        var types = items.Select(x => x.Value.GetType()).Distinct().ToList();
        var value = types.Count == 1 ? Array.CreateInstance(types[0], items.Length) : new object[items.Length];

        for (var i = 0; i < items.Length; i++)
        {
            value.SetValue(items[i].Value, i);
        }
            
        return value;
    }
    
    private static WireType ComputeWireType(ProtoValue[] elements)
    {
        var wireTypes = elements.Select(x => x.WireType).Distinct().ToList();
        
        return wireTypes.Count == 1 ? wireTypes[0] : WireType.None;
    }
}