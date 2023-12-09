using System.Diagnostics;
using System.Linq;

namespace ProtoUntyped;

internal class ProtoObjectDebugView
{
    private readonly ProtoObject _protoObject;

    public ProtoObjectDebugView(ProtoObject protoObject)
    {
        _protoObject = protoObject;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public ProtoField[] Items => _protoObject.Fields.ToArray();
}