using System;
using System.Text;

namespace ProtoUntyped.Tests;

public class StrictProtoFormatter : ProtoFormatter
{
    protected override void FormatUnknownValue(StringBuilder stringBuilder, object value)
    {
        throw new NotSupportedException($"Unknown: {value}");
    }
}