namespace ProtoUntyped;

internal class ProtoWireValueDebugView
{
    private readonly ProtoWireValue _wireValue;

    public ProtoWireValueDebugView(ProtoWireValue wireValue)
    {
        _wireValue = wireValue;
    }

    public ProtoWireValueType Type => _wireValue.Type;

    public object Value => _wireValue.GetValue();
}