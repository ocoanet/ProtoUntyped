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
            BuildString(stringBuilder, _indentIncrement, field.Value, field.WireType);
            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }

    internal string BuildString(ProtoWireField field)
    {
        var stringBuilder = new StringBuilder(1024);

        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", field.FieldNumber);
        BuildString(stringBuilder, _indentIncrement, field.Value, field.WireType);

        return stringBuilder.ToString();
    }

    private void BuildString(StringBuilder stringBuilder, int indentSize, ProtoWireValue value, WireType wireType)
    {
        switch (value.Type)
        {
            case ProtoWireValueType.Message:
                FormatMessage(stringBuilder, indentSize, value.MessageValue, wireType);
                break;

            case ProtoWireValueType.Bytes:
                FormatByteArray(stringBuilder, indentSize, value.BytesValue);
                break;

            case ProtoWireValueType.Int32 when wireType == WireType.Fixed32:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "0x{0:x}i32", value.Int32Value);
                break;

            case ProtoWireValueType.Int64 when wireType == WireType.Fixed64:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "0x{0:x}i64", value.Int64Value);
                break;

            case ProtoWireValueType.String:
                FormatString(stringBuilder, indentSize, value.StringValue);
                break;

            default:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", value);
                break;
        }
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
            BuildString(stringBuilder, indentSize + _indentIncrement, singleField.Value, singleField.WireType);
            stringBuilder.Append("}");
            return;
        }

        stringBuilder.AppendLine("{");

        foreach (var member in obj.Fields)
        {
            stringBuilder.Append(' ', indentSize);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}: ", member.FieldNumber);
            BuildString(stringBuilder, indentSize + _indentIncrement, member.Value, member.WireType);
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