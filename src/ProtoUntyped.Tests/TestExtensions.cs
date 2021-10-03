using KellermanSoftware.CompareNetObjects;
using Xunit;

namespace ProtoUntyped.Tests
{
    public static class TestExtensions
    {
        public static void ShouldEqual<T>(this T actual, T expected)
        {
            Assert.Equal(expected, actual);
        }
        
        public static void ShouldDeepEqual<T>(this T actual, T expected)
        {
            var comparer = new CompareLogic(new ComparisonConfig());

            var result = comparer.Compare(expected, actual);
            Assert.True(result.AreEqual, "Objects are not equal: " + result.DifferencesString);
        }
    }
}
