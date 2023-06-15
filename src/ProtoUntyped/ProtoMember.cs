namespace ProtoUntyped;

public abstract class ProtoMember
{
    protected ProtoMember(int fieldNumber, object value)
    {
        FieldNumber = fieldNumber;
        Value = value;
    }

    public int FieldNumber { get; }
    public object Value { get; }

    public abstract void Accept(ProtoObjectVisitor visitor);
}