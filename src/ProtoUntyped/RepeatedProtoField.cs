using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProtoBuf;

namespace ProtoUntyped;

[DebuggerTypeProxy(typeof(RepeatedProtoFieldDebugView))]
[DebuggerDisplay("FieldNumber = {FieldNumber}, Length = {Fields.Count}")]
public class RepeatedProtoField : ProtoField
{
    public RepeatedProtoField(ProtoField[] fields)
        : base(GetFieldNumber(fields), ComputeWireType(fields), ComputeValue(fields))
    {
        Fields = fields;
    }
    
    public RepeatedProtoField(int fieldNumber, WireType wireType, string[] values)
        : base(fieldNumber, wireType, values)
    {
        Fields = values.Select(x => new ProtoField(fieldNumber, wireType, x)).ToArray();
    }
    
    public RepeatedProtoField(int fieldNumber, WireType wireType, int[] values)
        : base(fieldNumber, wireType, values)
    {
        Fields = values.Select(x => new ProtoField(fieldNumber, wireType, x)).ToArray();
    }
    
    public RepeatedProtoField(int fieldNumber, WireType wireType, long[] values)
        : base(fieldNumber, wireType, values)
    {
        Fields = values.Select(x => new ProtoField(fieldNumber, wireType, x)).ToArray();
    }
    
    public RepeatedProtoField(int fieldNumber, WireType wireType, ProtoObject[] values)
        : base(fieldNumber, wireType, values)
    {
        Fields = values.Select(x => new ProtoField(fieldNumber, wireType, x)).ToArray();
    }

    internal RepeatedProtoField(int fieldNumber, ProtoField[] fields)
        : base(fieldNumber, ComputeWireType(fields), ComputeValue(fields))
    {
        Fields = fields;
    }

    public IReadOnlyList<ProtoField> Fields { get; }

    public override string ToString()
    {
        return ToString(ProtoFormatter.Default);
    }

    public string ToString(ProtoFormatter formatter)
    {
        return formatter.BuildString(this);
    }
    
    private static int GetFieldNumber(ProtoField[] fields)
    {
        if (fields.Length == 0)
            throw new ArgumentException("fields cannot be empty");

        var fieldNumber = fields[0].FieldNumber;

        for (int i = 1; i < fields.Length; i++)
        {
            if (fields[i].FieldNumber != fieldNumber)
                throw new ArgumentException("fields must have the same field number");
        }

        return fieldNumber;
    }

    private static Array ComputeValue(ProtoField[] fields)
    {
        var types = fields.Select(x => x.Value.GetType()).Distinct().ToList();
        var value = types.Count == 1 ? Array.CreateInstance(types[0], fields.Length) : new object[fields.Length];

        for (var i = 0; i < fields.Length; i++)
        {
            value.SetValue(fields[i].Value, i);
        }
        
        return value;
    }
    
    private static WireType ComputeWireType(ProtoField[] fields)
    {
        var wireTypes = fields.Select(x => x.WireType).Distinct().ToList();
        
        return wireTypes.Count == 1 ? wireTypes[0] : WireType.None;
    }
}