namespace ProtoUntyped;

internal static class ProtoUntypedDebuggerDisplay
{
    public static object GetDebugValue(ProtoField protoField)
    {
        return protoField.Value switch
        {
            ProtoObject o => $"Message {{ {GetDebugString(o)} }}",
            string s      => $"\"{s}\"",
            _             => protoField.Value,
        };
    }

    public static string GetDebugString(ProtoObject protoObject)
    {
        return $"FieldCount = {protoObject.Fields.Count}";
    }
    
    public static string GetDebugString(ProtoWireObject wireObject)
    {
        return $"FieldCount = {wireObject.Fields.Count}";
    }
}