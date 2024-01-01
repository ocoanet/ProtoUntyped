using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ProtoUntyped;

public class ProtoFormatter
{
    private readonly int _indentIncrement;

    public ProtoFormatter(int indentIncrement = 4)
    {
        _indentIncrement = indentIncrement;
    }

    public static ProtoFormatter Default { get; } = new();

    protected internal string BuildString(ProtoObject fieldContainer)
    {
        var stringBuilder = new StringBuilder(1024);
        BuildString(stringBuilder, 0, fieldContainer);
        stringBuilder.AppendLine();

        return stringBuilder.ToString();
    }

    protected internal string BuildString(ProtoField protoMember)
    {
        var stringBuilder = new StringBuilder(1024);

        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "- {0} = ", protoMember.FieldNumber);
        BuildString(stringBuilder, _indentIncrement, protoMember.Value);
        stringBuilder.AppendLine();

        return stringBuilder.ToString();
    }

    private void BuildString(StringBuilder stringBuilder, int indentSize, object value)
    {
        switch (value)
        {
            case string:
                FormatString(stringBuilder, indentSize, value);
                break;

            case byte[] array:
                FormatByteArray(stringBuilder, indentSize, array);
                break;

            case ProtoObject obj:
                FormatProtoObject(stringBuilder, indentSize, obj.Fields);
                break;

            case int:
                FormatNumericValue(stringBuilder, value);
                break;

            case long:
                FormatNumericValue(stringBuilder, value);
                break;

            case float:
                FormatNumericValue(stringBuilder, value);
                break;

            case double:
                FormatNumericValue(stringBuilder, value);
                break;

            case int[] array:
                FormatNumericArray(stringBuilder, indentSize, array);
                break;

            case long[] array:
                FormatNumericArray(stringBuilder, indentSize, array);
                break;

            case float[] array:
                FormatNumericArray(stringBuilder, indentSize, array);
                break;

            case double[] array:
                FormatNumericArray(stringBuilder, indentSize, array);
                break;

            case decimal:
                FormatString(stringBuilder, indentSize, value);
                break;

            case DateTime dateTime:
                FormatDateTime(stringBuilder, indentSize, dateTime);
                break;

            case TimeSpan timeSpan:
                FormatTimeSpan(stringBuilder, indentSize, timeSpan);
                break;

            case Guid:
                FormatString(stringBuilder, indentSize, value);
                break;

            // Repeated values

            case decimal[] array:
                FormatNumericArray(stringBuilder, indentSize, array);
                break;

            case IList array:
                FormatArray(stringBuilder, indentSize, array);
                break;

            default:
                FormatUnknownValue(stringBuilder, value);
                break;
        }
    }

    protected virtual void FormatNumericValue(StringBuilder stringBuilder, object value)
    {
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", value);
    }
    
    protected virtual void FormatUnknownValue(StringBuilder stringBuilder, object value)
    {
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", value);
    }

    protected virtual void FormatArray(StringBuilder stringBuilder, int indentSize, IList array)
    {
        stringBuilder.AppendLine("array [");
        foreach (var item in array)
        {
            stringBuilder.Append(' ', indentSize);
            BuildString(stringBuilder, indentSize + _indentIncrement, item);
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(' ', indentSize);
        stringBuilder.Append("]");
    }

    protected virtual void FormatByteArray(StringBuilder stringBuilder, int indentSize, byte[] array)
    {
        stringBuilder.Append(Convert.ToBase64String(array));
    }

    protected virtual void FormatProtoObject(StringBuilder stringBuilder, int indentSize, IReadOnlyList<ProtoField> fields)
    {
        stringBuilder.AppendLine("message {");
        foreach (var member in fields)
        {
            stringBuilder.Append(' ', indentSize);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "- {0} = ", member.FieldNumber);
            BuildString(stringBuilder, indentSize + _indentIncrement, member.Value);
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(' ', indentSize);
        stringBuilder.Append("}");
    }

    protected virtual void FormatNumericArray(StringBuilder stringBuilder, int indentSize, IList array)
    {
        stringBuilder.Append("array [ ");
        for (var index = 0; index < array.Count; index++)
        {
            if (index != 0)
                stringBuilder.Append(" ");

            FormatNumericValue(stringBuilder, array[index]);
        }

        stringBuilder.Append(" ]");
    }

    protected virtual void FormatString(StringBuilder stringBuilder, int indentSize, object value)
    {
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\"", value);
    }

    protected virtual void FormatDateTime(StringBuilder stringBuilder, int indentSize, DateTime dateTime)
    {
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, GetDefaultFormat(dateTime), dateTime);
    }

    protected virtual void FormatTimeSpan(StringBuilder stringBuilder, int indentSize, TimeSpan timeSpan)
    {
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, GetDefaultFormat(timeSpan), timeSpan);
    }

    public static string GetDefaultFormat(DateTime dateTime)
    {
        return dateTime switch
        {
            { TimeOfDay.Ticks: 0 }        => "\"{0:yyyy-MM-dd}\"",
            { TimeOfDay.Milliseconds: 0 } => "\"{0:yyyy-MM-dd} {0:HH\\:mm\\:ss}\"",
            _                             => "\"{0:yyyy-MM-dd} {0:HH\\:mm\\:ss\\.fff}\""
        };
    }

    public static string GetDefaultFormat(TimeSpan timeSpan)
    {
        return timeSpan switch
        {
            { Days: 0 } and { Milliseconds: 0 } => "\"{0:hh\\:mm\\:ss}\"",
            { Days: 0 }                         => "\"{0:hh\\:mm\\:ss\\.fff}\"",
            { Milliseconds: 0 }                 => "\"{0:d\\.hh\\:mm\\:ss}\"",
            _                                   => "\"{0:d\\.hh\\:mm\\:ss\\.fff}\""
        };
    }
}