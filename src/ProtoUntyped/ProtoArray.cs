using System;
using System.Linq;

namespace ProtoUntyped;

public class ProtoArray : ProtoMember
{
    public ProtoArray(int fieldNumber, ProtoValue[] items)
        : base(fieldNumber, BuildValue(items))
    {
        Items = items;
    }

    public ProtoValue[] Items { get; }

    public override string ToString()
    {
        return ToString(ProtoFormatter.Default);
    }

    public string ToString(ProtoFormatter formatter)
    {
        return formatter.BuildString(this);
    }

    private static object BuildValue(ProtoValue[] items)
    {
        var types = items.Select(x => x.Value.GetType()).Distinct().ToList();
        var value = types.Count == 1 ? Array.CreateInstance(types[0], items.Length) : new object[items.Length];

        for (var i = 0; i < items.Length; i++)
        {
            value.SetValue(items[i].Value, i);
        }
            
        return value;
    }
}