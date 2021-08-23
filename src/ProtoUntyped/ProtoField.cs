using System;
using System.Collections.Generic;

namespace ProtoUntyped
{
    public class ProtoField : IEquatable<ProtoField>
    {
        public ProtoField(int fieldNumber, object value)
        {
            FieldNumber = fieldNumber;
            Value = value;
        }

        public int FieldNumber { get; }
        public object Value { get; private set; }

        public void Add(object value)
        {
            if (Value is not List<object> list)
                Value = list = new List<object> { Value };

            list.Add(value);
        }

        public bool Equals(ProtoField other)
        {
            return other != null && FieldNumber == other.FieldNumber && Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is ProtoField other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FieldNumber, Value);
        }
    }
}
