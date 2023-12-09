using System.Diagnostics;
using System.Linq;

namespace ProtoUntyped;

internal class ProtoWireObjectDebugView
{
    private readonly ProtoWireObject _wireObject;

    public ProtoWireObjectDebugView(ProtoWireObject wireObject)
    {
        _wireObject = wireObject;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public ProtoWireField[] Items => _wireObject.Fields.ToArray();
}