using System;
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
    public object[] Items => _repeatedProtoField.Fields.Select(x => x.Value).ToArray();
}
