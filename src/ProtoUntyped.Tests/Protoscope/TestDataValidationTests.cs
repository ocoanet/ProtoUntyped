using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ProtoUntyped.Tests.Protoscope;

public class TestDataValidationTests
{
    [Fact]
    public void ShouldFindProtoscopeBinaryFiles()
    {
        var filePaths = GetProtoscopeBinaryFilePaths();
        
        filePaths.Length.ShouldNotEqual(0);
    }

    [Theory]
    [MemberData(nameof(GetProtoscopeBinaryFilePathsData))]
    public void ShouldDecodeProtoscopeBinaryFile(TestDataPath filePath)
    {
        var data = File.ReadAllBytes(filePath.Value);
        var decodeOptions = new ProtoDecodeOptions();
            
        var succeeded = ProtoObject.TryDecode(data, decodeOptions, out var protoObject);
            
        succeeded.ShouldEqual(true);
    }

    public static IEnumerable<object[]> GetProtoscopeBinaryFilePathsData()
    {
        return GetProtoscopeBinaryFilePaths().Select(x => new object[] { x });
    }

    private static TestDataPath[] GetProtoscopeBinaryFilePaths()
    {
        var directoryPath = Path.Combine(AppContext.BaseDirectory, "Protoscope", "TestData");
        var filePaths = Directory.GetFiles(directoryPath, "*.pb");
        
        var excludedFileNames = new[]
        {
            "groups.pb", // Not supported: contains invalid start and end group tags
        };
        
        return filePaths.Where(x => !excludedFileNames.Contains(Path.GetFileName(x)))
                        .Select(x => new TestDataPath(x))
                        .ToArray();
    }

    public readonly record struct TestDataPath(string Value)
    {
        public override string ToString()
        {
            return Path.GetFileName(Value);
        }
    }
}
