using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ProtoUntyped.Decoders;

internal static class ProtoEncoder
{
    public static ProtoWireObject Encode(ProtoObject protoObject)
    {
        if (TryEncode(protoObject, out var protoWireObject))
            return protoWireObject;
        
        throw new ArgumentException("Unable to encode object");
    }
    
    public static bool TryEncode(ProtoObject protoObject, out ProtoWireObject protoWireObject)
    {
        if (TryEncodeFields(protoObject.GetNormalizedFields(), out var wireFields))
        {
            protoWireObject = new ProtoWireObject(wireFields);
            return true;
        }
        
        protoWireObject = null!;
        return false;
    }

    private static bool TryEncodeFields(IEnumerable<ProtoField> fields, out List<ProtoWireField> wireFields)
    {
        wireFields = new List<ProtoWireField>();

        foreach (var field in fields)
        {
            var wireField = EncodeField(field);
            if (wireField == null)
                return false;
            
            wireFields.Add(wireField);
        }

        return true;
    }

    private static ProtoWireField? EncodeField(ProtoField field)
    {
        switch (field.Value)
        {
            case string s:
                return CreateWireField(field, new ProtoWireValue(s));

            case byte[] array:
                return CreateWireField(field, new ProtoWireValue(array));
            
            case ProtoObject obj:
                return TryEncode(obj, out var wireObject) ? CreateWireField(field, new ProtoWireValue(wireObject)) : null;
            
            case int value:
                return CreateWireField(field, new ProtoWireValue(value));

            case long value:
                return CreateWireField(field, new ProtoWireValue(value));
            
            case float value:
                return CreateWireField(field, new ProtoWireValue(Unsafe.As<float, int>(ref value)));

            case double value:
                return CreateWireField(field, new ProtoWireValue(Unsafe.As<double, long>(ref value)));
            
            case int[] array:
                return CreateWireField(field, new ProtoWireValue(array));

            case long[] array:
                return CreateWireField(field, new ProtoWireValue(array));

            case float[] array:
                return CreateWireField(field, new ProtoWireValue(ToInt32Array(array)));

            case double[] array:
                return CreateWireField(field, new ProtoWireValue(ToInt64Array(array)));
            
            case decimal d:
                return CreateWireField(field, new ProtoWireValue(DecimalDecoder.EncodeDecimal(d)));
   
            case DateTime dateTime:
                return CreateWireField(field, new ProtoWireValue(TimeDecoder.EncodeDateTime(dateTime)));

            case TimeSpan timeSpan:
                return CreateWireField(field, new ProtoWireValue(TimeDecoder.EncodeTimeSpan(timeSpan)));
            
            case Guid guid:
                return CreateWireField(field, new ProtoWireValue(GuidDecoder.EncodeGuid(guid)));

            default:
                return null;
        }
    }

    private static int[] ToInt32Array(float[] array)
    {
        var result = new int[array.Length];
        
        for (var i = 0; i < array.Length; i++)
        {
            result[i] = Unsafe.As<float, int>(ref array[i]);
        }

        return result;
    }
    
    private static long[] ToInt64Array(double[] array)
    {
        var result = new long[array.Length];
        
        for (var i = 0; i < array.Length; i++)
        {
            result[i] = Unsafe.As<double, long>(ref array[i]);
        }

        return result;
    }

    private static ProtoWireField CreateWireField(ProtoField field, ProtoWireValue value)
    {
        return new ProtoWireField(field.FieldNumber, value, field.WireType, field.PackedWireType);
    }
}