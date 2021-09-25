using System.Collections.Generic;
using System.Linq;

namespace ProtoUntyped
{
    public class ProtoArray : ProtoMember
    {
        public ProtoArray(int fieldNumber, ProtoValue[] items)
            : base(fieldNumber, items.Select(x => x.Value).ToArray())
        {
            Items = items;
        }

        public ProtoValue[] Items { get; }
    }
}
