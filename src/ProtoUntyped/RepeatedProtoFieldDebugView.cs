using System.Diagnostics;
using System.Linq;

namespace ProtoUntyped;

internal class RepeatedProtoFieldDebugView
{
    private readonly RepeatedProtoField _repeatedProtoField;

    public RepeatedProtoFieldDebugView(RepeatedProtoField repeatedProtoField)
    {
        _repeatedProtoField = repeatedProtoField;
    }
    
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object[] Items => _repeatedProtoField.Elements.Select(x => x.Value).ToArray();
}
