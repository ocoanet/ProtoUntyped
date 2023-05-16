using System;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Xunit;

namespace ProtoUntyped.Tests
{
    public static class TestExtensions
    {
        public static void ShouldContain(this string actual, string expectedSubstring)
        {
            Assert.Contains(expectedSubstring, actual);
        }
        
        public static void ShouldEqual<T>(this T actual, T expected)
        {
            Assert.Equal(expected, actual);
        }
        
        public static void ShouldDeepEqual<T>(this T actual, T expected)
        {
            var comparer = new CompareLogic(new ComparisonConfig());
            comparer.Config.CustomComparers.Add(new EquatableComparer<DateTime>());
            comparer.Config.CustomComparers.Add(new EquatableComparer<TimeSpan>());

            var result = comparer.Compare(expected, actual);
            Assert.True(result.AreEqual, "Objects are not equal: " + result.DifferencesString);
        }
        
        private class EquatableComparer<T> : BaseTypeComparer
            where T : IEquatable<T>
        {
            public EquatableComparer()
                : base(RootComparerFactory.GetRootComparer())
            {
            }

            public override bool IsTypeMatch(Type type1, Type type2)
            {
                return type1 == typeof(T) && type2 == typeof(T);
            }

            public override void CompareType(CompareParms parms)
            {
                var obj1 = (T)parms.Object1;
                var obj2 = (T)parms.Object2;
                
                if (obj1.Equals(obj2))
                    return;

                AddDifference(parms);
            }
        }
    }
}
