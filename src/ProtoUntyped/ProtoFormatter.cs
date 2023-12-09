using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        return stringBuilder.ToString();
    }
    
    protected internal string BuildString(ProtoField protoMember)
    {
        var stringBuilder = new StringBuilder(1024);
        
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "- {0} = ", protoMember.FieldNumber);
        BuildString(stringBuilder, _indentIncrement, protoMember.Value);

        return stringBuilder.ToString();
    }

    private void BuildString(StringBuilder stringBuilder, int indentSize, object value)
    {
        switch (value)
        {
            case ProtoObject obj:
                FormatProtoObject(stringBuilder, indentSize, obj.Fields);
                break;
                
            case byte[] array:
                FormatByteArray(stringBuilder, indentSize, array);
                break;

            case float[] array:
                FormatNumericArray(stringBuilder, indentSize, array);
                break;
                
            case double[] array:
                FormatNumericArray(stringBuilder, indentSize, array);
                break;
                
            case long[] array:
                FormatNumericArray(stringBuilder, indentSize, array);
                break;

            case decimal[] array:
                FormatNumericArray(stringBuilder, indentSize, array);
                break;
                    
            case object[] array:
                FormatObjectArray(stringBuilder, indentSize, array);
                break;
                
            case string:
            case Guid:
                FormatString(stringBuilder, indentSize, value);
                break;
                
            case DateTime dateTime:
                FormatDateTime(stringBuilder, indentSize, dateTime);
                break;
                    
            case TimeSpan timeSpan:
                FormatTimeSpan(stringBuilder, indentSize, timeSpan);
                break;

            default:
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", value);
                stringBuilder.AppendLine();
                break;
        }
    }

    private void FormatObjectArray(StringBuilder stringBuilder, int indentSize, object[] array)
    {
        stringBuilder.AppendLine("array [");
        foreach (var (item, index) in array.Select((item, index) => (item, index)))
        {
            stringBuilder.Append(' ', indentSize);
            stringBuilder.AppendFormat("- {0} = ", index);
            BuildString(stringBuilder, indentSize + _indentIncrement, item);
        }

        stringBuilder.Append(' ', indentSize);
        stringBuilder.AppendLine("]");
    }

    protected virtual void FormatByteArray(StringBuilder stringBuilder, int indentSize, byte[] array)
    {
        stringBuilder.Append(Convert.ToBase64String(array));
        stringBuilder.AppendLine();
    }

    protected virtual void FormatProtoObject(StringBuilder stringBuilder, int indentSize, IReadOnlyList<ProtoField> fields)
    {
        stringBuilder.AppendLine("message {");
        foreach (var member in fields)
        {
            stringBuilder.Append(' ', indentSize);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "- {0} = ", member.FieldNumber);
            BuildString(stringBuilder, indentSize + _indentIncrement, member.Value);
        }

        stringBuilder.Append(' ', indentSize);
        stringBuilder.AppendLine("}");
    }

    protected virtual void FormatNumericArray(StringBuilder stringBuilder, int indentSize, IList array)
    {
        stringBuilder.Append("array [ ");
        for (var index = 0; index < array.Count; index++)
        {
            if (index != 0)
                stringBuilder.Append(" ");

            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", array[index]);   
        }

        stringBuilder.AppendLine(" ]");
    }

    protected virtual void FormatString(StringBuilder stringBuilder, int indentSize, object value)
    {
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\"", value);
        stringBuilder.AppendLine();
    }

    protected virtual void FormatDateTime(StringBuilder stringBuilder, int indentSize, DateTime dateTime)
    {
        if (dateTime.TimeOfDay == default)
        {
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:yyyy-MM-dd}\"", dateTime);
            stringBuilder.AppendLine();    
        }
        else if (dateTime.TimeOfDay.Milliseconds == default)
        {
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:yyyy-MM-dd} {1:hh\\:mm\\:ss}\"", dateTime, dateTime.TimeOfDay);
            stringBuilder.AppendLine();    
        }
        else
        {
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:yyyy-MM-dd} {1:hh\\:mm\\:ss\\.fff}\"", dateTime, dateTime.TimeOfDay);
            stringBuilder.AppendLine();
        }
    }

    protected virtual void FormatTimeSpan(StringBuilder stringBuilder, int indentSize, TimeSpan timeSpan)
    {
        if (timeSpan.Days == default)
        {
            if (timeSpan.Milliseconds == default)
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:hh\\:mm\\:ss}\"", timeSpan);
                stringBuilder.AppendLine();    
            }
            else
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:hh\\:mm\\:ss\\.fff}\"", timeSpan);
                stringBuilder.AppendLine();        
            }
        }
        else
        {
            if (timeSpan.Milliseconds == default)
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:d\\.hh\\:mm\\:ss}\"", timeSpan);
                stringBuilder.AppendLine();        
            }
            else
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:d\\.hh\\:mm\\:ss\\.fff}\"", timeSpan);
                stringBuilder.AppendLine();        
            }
        }
    }
}