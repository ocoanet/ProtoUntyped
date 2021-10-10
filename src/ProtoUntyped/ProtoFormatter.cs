using System;
using System.Globalization;
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
                    stringBuilder.AppendLine("[message]");
                    foreach (var member in obj.Members)
                    {
                        stringBuilder.Append(' ', indentSize);
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "- {0} = ", member.FieldNumber);
                        BuildString(member.Value, stringBuilder, indentSize + IndentIncrement);
                    }
                    break;
                    
                case object[] array:
                    stringBuilder.AppendLine();
                    foreach (var item in array)
                    {
                        stringBuilder.Append(' ', indentSize);
                        stringBuilder.Append("- ");
                        BuildString(item, stringBuilder, indentSize + IndentIncrement);
                    }
                    break;
                
                case string:
                case Guid:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\"", value);
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
