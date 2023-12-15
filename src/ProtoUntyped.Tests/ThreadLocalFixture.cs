using System;
using System.Linq;
using System.Threading;
using AutoFixture;
using AutoFixture.Kernel;

namespace ProtoUntyped.Tests;

public static class ThreadLocalFixture
{
    private static readonly ThreadLocal<Fixture> _instance = new(() => new Fixture());

    public static Fixture Instance => _instance.Value;

    public static T Create<T>() => Instance.Create<T>();
    public static T[] CreateMany<T>(int count) => Instance.CreateMany<T>(count).ToArray();
    public static object Create(Type type) => new SpecimenContext(Instance).Resolve(type);
}