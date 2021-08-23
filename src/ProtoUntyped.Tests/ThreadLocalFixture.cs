using System.Threading;
using AutoFixture;

namespace ProtoUntyped.Tests
{
    public static class ThreadLocalFixture
    {
        private static readonly ThreadLocal<Fixture> _instance = new(() => new Fixture());

        public static Fixture Instance => _instance.Value;

        public static T Create<T>() => Instance.Create<T>();
    }
}
