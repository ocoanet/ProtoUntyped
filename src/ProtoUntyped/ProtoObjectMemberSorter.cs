namespace ProtoUntyped;

internal class ProtoObjectMemberSorter : ProtoObjectVisitor
{
    public static ProtoObjectMemberSorter Instance { get; } = new();

    public override void Visit(ProtoObject protoObject)
    {
        protoObject.SortMembers();
    }
}
