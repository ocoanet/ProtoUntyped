using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace ProtoUntyped;

public class ProtoscopeFormatter
{
    private readonly int _indentIncrement;
    private readonly string _fixed32Format;
    private readonly string _fixed64Format;

    public ProtoscopeFormatter(int indentIncrement = 2, string fixed32Format = "0x{0:x}i32", string fixed64Format = "0x{0:x}i64")
    {
        _indentIncrement = indentIncrement;
        _fixed32Format = fixed32Format;
        _fixed64Format = fixed64Format;
    }

    public static ProtoscopeFormatter Default { get; } = new();

    internal string BuildString(ProtoObject obj)
    {
        var stringBuilder = new StringBuilder(1024);

        foreach (var field in ExpandFields(obj))
        {
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", field.FieldNumber);
            BuildString(stringBuilder, _indentIncrement, field);
            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }

    internal string BuildString(ProtoWireObject obj)
    {
        var stringBuilder = new StringBuilder(1024);

        foreach (var field in obj.Fields)
        {
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", field.FieldNumber);
            BuildString(stringBuilder, _indentIncrement, field);
            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }

    private void BuildString(StringBuilder stringBuilder, int indentSize, ProtoWireField field)
    {
        switch (field.Value.Type)
        {
            case ProtoWireValueType.Message:
                FormatMessage(stringBuilder, indentSize, field.Value.MessageValue, field.WireType);
                break;

            case ProtoWireValueType.Bytes:
                FormatByteArray(stringBuilder, indentSize, field.Value.BytesValue);
                break;

            case ProtoWireValueType.Int32 when field.WireType == WireType.Fixed32:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, _fixed32Format, field.Value.Int32Value);
                break;

            case ProtoWireValueType.Int64 when field.WireType == WireType.Fixed64:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, _fixed64Format, field.Value.Int64Value);
                break;

            case ProtoWireValueType.String:
                FormatString(stringBuilder, field.Value.StringValue);
                break;
            
            case ProtoWireValueType.Int64Array when field.PackedWireType == WireType.Varint:
            case ProtoWireValueType.Int64Array when field.PackedWireType == WireType.SignedVarint:
                FormatNumberArray(stringBuilder, field.Value.Int64ArrayValue, "{0}");
                break;
            
            case ProtoWireValueType.Int32Array when field.PackedWireType == WireType.Fixed32:
                FormatNumberArray(stringBuilder, field.Value.Int32ArrayValue, _fixed32Format);
                break;
            
            case ProtoWireValueType.Int64Array when field.PackedWireType == WireType.Fixed64:
                FormatNumberArray(stringBuilder, field.Value.Int64ArrayValue, _fixed64Format);
                break;

            default:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", field.Value);
                break;
        }
    }

    protected virtual void FormatNumberArray<T>(StringBuilder stringBuilder, T[] array, string format)
    {
        if (array.Length == 0)
        {
            stringBuilder.Append("{}");
            return;
        }
        
        stringBuilder.Append("{ ");

        foreach (var item in array)
        {
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, format, item);
            stringBuilder.Append(" ");
        }
        
        stringBuilder.Append("}");
    }

    protected virtual void FormatByteArray(StringBuilder stringBuilder, int indentSize, byte[] bytes)
    {
        const int maxLengthChars = 80;
        const int maxLengthBytes = maxLengthChars / 2;
        if (bytes.Length < maxLengthBytes)
        {
            stringBuilder.Append($"{{`{ToHexString(bytes, 0, bytes.Length)}`}}");
            return;
        }
        
        stringBuilder.AppendLine("{");

        for (var index = 0; index < bytes.Length; index += maxLengthBytes)
        {
            stringBuilder.Append(' ', indentSize);
            stringBuilder.Append("`");
            stringBuilder.Append(ToHexString(bytes, index, Math.Min(index + maxLengthBytes, bytes.Length)));
            stringBuilder.AppendLine("`");
        }
        
        stringBuilder.Append(' ', indentSize - _indentIncrement);
        stringBuilder.Append("}");

        static string ToHexString(byte[] bytes, int start, int end)
        {
            var hex = new StringBuilder(2 * (end - start));
            for (var index = start; index < end; index++)
            {
                hex.AppendFormat("{0:x2}", bytes[index]);
            }

            return hex.ToString();
        }
    }

    protected virtual void FormatMessage(StringBuilder stringBuilder, int indentSize, ProtoWireObject obj, WireType wireType)
    {
        if (wireType == WireType.StartGroup)
            stringBuilder.Append("!");

        if (obj.Fields.Count == 0)
        {
            stringBuilder.Append("{}");
            return;
        }

        if (obj.Fields.Count == 1 && obj.Fields[0].Value.Type != ProtoWireValueType.Message)
        {
            var singleField = obj.Fields[0];

            stringBuilder.Append("{");
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", singleField.FieldNumber);
            BuildString(stringBuilder, indentSize + _indentIncrement, singleField);
            stringBuilder.Append("}");
            return;
        }

        stringBuilder.AppendLine("{");

        foreach (var member in obj.Fields)
        {
            stringBuilder.Append(' ', indentSize);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", member.FieldNumber);
            BuildString(stringBuilder, indentSize + _indentIncrement, member);
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(' ', indentSize - _indentIncrement);
        stringBuilder.Append("}");
    }

    protected virtual void FormatString(StringBuilder stringBuilder, string value)
    {
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{{\"{0}\"}}", value);
    }
    
    private void BuildString(StringBuilder stringBuilder, int indentSize, ProtoField field)
    {
        switch (field.Value)
        {
            case ProtoObject obj:
                FormatMessage(stringBuilder, indentSize, obj, field.WireType);
                break;
                
            case byte[] array:
                FormatByteArray(stringBuilder, indentSize, array);
                break;

            case float[] array:
                FormatNumberArray(stringBuilder, array, "{0}f");
                break;
                
            case double[] array:
                FormatNumberArray(stringBuilder, array, "{0}d");
                break;
                
            case int[] array when field.PackedWireType == WireType.Fixed32 :
                FormatNumberArray(stringBuilder, array, _fixed32Format);
                break;
            
            case int[] array:
                FormatNumberArray(stringBuilder, array, "{0}");
                break;
            
            case long[] array when field.PackedWireType == WireType.Fixed64:
                FormatNumberArray(stringBuilder, array, _fixed64Format);
                break;
            
            case long[] array:
                FormatNumberArray(stringBuilder, array, "{0}");
                break;

            case decimal[] array:
                FormatNumberArray(stringBuilder, array, "{0}m");
                break;
            
            case int value when field.WireType == WireType.Fixed32:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, _fixed32Format, value);
                break;

            case long value when field.WireType == WireType.Fixed64:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, _fixed64Format, value);
                break;
                
            case string:
            case Guid:
                FormatString(stringBuilder, field.Value.ToString());
                break;
                
            case DateTime dateTime:
                FormatDateTime(stringBuilder, indentSize, dateTime);
                break;
                    
            case TimeSpan timeSpan:
                FormatTimeSpan(stringBuilder, indentSize, timeSpan);
                break;

            default:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", field.Value);
                break;
        }
    }
    
    protected virtual void FormatMessage(StringBuilder stringBuilder, int indentSize, ProtoObject obj, WireType wireType)
    {
        if (wireType == WireType.StartGroup)
            stringBuilder.Append("!");

        var normalizedFields = ExpandFields(obj).ToList();

        if (normalizedFields.Count == 0)
        {
            stringBuilder.Append("{}");
            return;
        }

        if (normalizedFields.Count == 1 && normalizedFields[0].Value is not ProtoObject)
        {
            var singleField = normalizedFields[0];

            stringBuilder.Append("{");
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", singleField.FieldNumber);
            BuildString(stringBuilder, indentSize + _indentIncrement, singleField);
            stringBuilder.Append("}");
            return;
        }

        stringBuilder.AppendLine("{");

        foreach (var member in normalizedFields)
        {
            stringBuilder.Append(' ', indentSize);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", member.FieldNumber);
            BuildString(stringBuilder, indentSize + _indentIncrement, member);
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(' ', indentSize - _indentIncrement);
        stringBuilder.Append("}");
    }
    
    protected virtual void FormatDateTime(StringBuilder stringBuilder, int indentSize, DateTime dateTime)
    {
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ProtoFormatter.GetDefaultFormat(dateTime), dateTime);
    }

    protected virtual void FormatTimeSpan(StringBuilder stringBuilder, int indentSize, TimeSpan timeSpan)
    {
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ProtoFormatter.GetDefaultFormat(timeSpan), timeSpan);
    }

    protected static IEnumerable<ProtoField> ExpandFields(ProtoObject protoObject)
    {
        return protoObject.Fields.SelectMany(Expand);

        IEnumerable<ProtoField> Expand(ProtoField protoField)
        {
            if (protoField.Value is decimal[] or int[] or long[] or float[] or double[])
                return new[] { protoField };

            if (protoField is RepeatedProtoField repeatedProtoField)
                return repeatedProtoField.Fields;

            return new[] { protoField };
        }
    }
}