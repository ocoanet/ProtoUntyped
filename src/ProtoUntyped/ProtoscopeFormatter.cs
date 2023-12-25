using System;
using System.Collections;
using System.Globalization;
using System.Text;
using ProtoBuf;

namespace ProtoUntyped;

public class ProtoscopeFormatter
{
    private readonly int _indentIncrement;

    public ProtoscopeFormatter(int indentIncrement = 2)
    {
        _indentIncrement = indentIncrement;
    }

    public static ProtoscopeFormatter Default { get; } = new();

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

    internal string BuildString(ProtoWireField field)
    {
        var stringBuilder = new StringBuilder(1024);

        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", field.FieldNumber);
        BuildString(stringBuilder, _indentIncrement, field);

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
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "0x{0:x}i32", field.Value.Int32Value);
                break;

            case ProtoWireValueType.Int64 when field.WireType == WireType.Fixed64:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "0x{0:x}i64", field.Value.Int64Value);
                break;

            case ProtoWireValueType.String:
                FormatString(stringBuilder, indentSize, field.Value.StringValue);
                break;
            
            case ProtoWireValueType.Int64Array when field.PackedWireType == WireType.Varint:
            case ProtoWireValueType.Int64Array when field.PackedWireType == WireType.SignedVarint:
                FormatNumberArray(stringBuilder, field.Value.Int64ArrayValue, "{0}");
                break;
            
            case ProtoWireValueType.Int32Array when field.PackedWireType == WireType.Fixed32:
                FormatNumberArray(stringBuilder, field.Value.Int32ArrayValue, "0x{0:x}i32");
                break;
            
            case ProtoWireValueType.Int64Array when field.PackedWireType == WireType.Fixed64:
                FormatNumberArray(stringBuilder, field.Value.Int64ArrayValue, "0x{0:x}i64");
                break;

            default:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", field.Value);
                break;
        }
    }

    private static void FormatNumberArray<T>(StringBuilder stringBuilder, T[] array, string format)
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

    private void FormatByteArray(StringBuilder stringBuilder, int indentSize, byte[] bytes)
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

    private void FormatMessage(StringBuilder stringBuilder, int indentSize, ProtoWireObject obj, WireType wireType)
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

    private void FormatString(StringBuilder stringBuilder, int indentSize, string value)
    {
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{{\"{0}\"}}", value);
    }
}