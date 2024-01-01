using System;
using System.Text;

namespace ProtoUntyped.Tests;

public class StrictProtoscopeFormatter : ProtoscopeFormatter
{
    protected override void FormatUnknownValue(StringBuilder stringBuilder, ProtoField field)
    {
        throw new NotSupportedException($"Unknown: {field.Value}");
    }
}