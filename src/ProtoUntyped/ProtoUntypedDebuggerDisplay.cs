namespace ProtoUntyped;

internal static class ProtoUntypedDebuggerDisplay
{
    public static object GetDebugValue(ProtoField protoField)
    {
        return protoField.Value switch
        {
            ProtoObject protoObject => $"ProtoObject {{ {GetDebugString(protoObject)} }}",
            string s                => $"\"{s}\"",
            _                       => protoField.Value,
        };
    }

    public static string GetDebugString(ProtoObject protoObject)
    {
        return $"FieldCount = {protoObject.Fields.Count}";
    }
}
