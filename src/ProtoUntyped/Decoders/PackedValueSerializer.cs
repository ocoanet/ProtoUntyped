using System;
using ProtoBuf;
using ProtoBuf.Serializers;

namespace ProtoUntyped.Decoders;

internal class PackedValueSerializer<T> : ISerializer<T>
    where T : struct
{
    public static readonly PackedValueSerializer<T> Instance = new();
        
    public T Read(ref ProtoReader.State state, T value)
    {
        if (typeof(T) == typeof(int))
            return (T)(object)state.ReadInt32();
            
        if (typeof(T) == typeof(long))
            return (T)(object)state.ReadInt64();

        return default;
    }

    public void Write(ref ProtoWriter.State state, T value)
    {
        if (typeof(T) == typeof(int))
        {
            state.WriteInt32((int)(object)value);
            return;
        }
            
        if (typeof(T) == typeof(long))
        {
            state.WriteInt64((long)(object)value);
            return;
        }
    }

    public SerializerFeatures Features => SerializerFeatures.CategoryScalar;
}