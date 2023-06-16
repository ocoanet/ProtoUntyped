using System.Diagnostics;
using System.Linq;

namespace ProtoUntyped;

internal class ProtoArrayDebugView
{
    private readonly ProtoArray _protoArray;

    public ProtoArrayDebugView(ProtoArray protoArray)
    {
        _protoArray = protoArray;
    }
    
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object[] Items => _protoArray.Items.Select(x => x.Value).ToArray();
}
