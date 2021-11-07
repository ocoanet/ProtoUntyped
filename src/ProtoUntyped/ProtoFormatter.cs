using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ProtoUntyped
{
    internal static class ProtoFormatter
    {
        public const int IndentIncrement = 4;

        public static string BuildString(object value)
        {
            var stringBuilder = new StringBuilder(1024);
            BuildString(value, stringBuilder, 0);

            return stringBuilder.ToString();
        }
        
        public static void BuildString(object value, StringBuilder stringBuilder, int indentSize)
        {
            switch (value)
            {
                case ProtoObject obj:
                    stringBuilder.AppendLine("message {");
                    foreach (var member in obj.Members)
                    {
                        stringBuilder.Append(' ', indentSize);
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "- {0} = ", member.FieldNumber);
                        BuildString(member.Value, stringBuilder, indentSize + IndentIncrement);
                    }
                    stringBuilder.Append(' ', indentSize);
                    stringBuilder.AppendLine("}");
                    break;
                    
                case object[] array:
                    stringBuilder.AppendLine("array [");
                    foreach (var (item, index) in array.Select((item, index) => (item, index)))
                    {
                        stringBuilder.Append(' ', indentSize);
                        stringBuilder.AppendFormat("- {0} = ", index);
                        BuildString(item, stringBuilder, indentSize + IndentIncrement);
                    }
                    stringBuilder.Append(' ', indentSize);
                    stringBuilder.AppendLine("]");
                    break;
                
                case string:
                case Guid:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\"", value);
                    stringBuilder.AppendLine();
                    break;
                
                case DateTime dateTime when dateTime.TimeOfDay == default:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:yyyy-MM-dd}\"", value);
                    stringBuilder.AppendLine();
                    break;
                
                case DateTime { TimeOfDay: { Milliseconds: 0 } } dateTime:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:yyyy-MM-dd} {1:hh\\:mm\\:ss}\"", value, dateTime.TimeOfDay);
                    stringBuilder.AppendLine();
                    break;
                
                case DateTime { TimeOfDay: { Milliseconds: not 0 } } dateTime:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:yyyy-MM-dd} {1:hh\\:mm\\:ss\\.fff}\"", value, dateTime.TimeOfDay);
                    stringBuilder.AppendLine();
                    break;
                
                case TimeSpan { Milliseconds: 0, Days: 0 } timeSpan:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:hh\\:mm\\:ss}\"", timeSpan);
                    stringBuilder.AppendLine();
                    break;
                
                case TimeSpan { Milliseconds: not 0, Days: 0 } timeSpan:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:hh\\:mm\\:ss\\.fff}\"", timeSpan);
                    stringBuilder.AppendLine();
                    break;
                
                case TimeSpan { Milliseconds: 0, Days: not 0 } timeSpan:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:d\\.hh\\:mm\\:ss}\"", timeSpan);
                    stringBuilder.AppendLine();
                    break;
                
                case TimeSpan { Milliseconds: not 0, Days: not 0 } timeSpan:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0:d\\.hh\\:mm\\:ss\\.fff}\"", timeSpan);
                    stringBuilder.AppendLine();
                    break;

                default:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", value);
                    stringBuilder.AppendLine();
                    break;
            }
        }
    }
}
