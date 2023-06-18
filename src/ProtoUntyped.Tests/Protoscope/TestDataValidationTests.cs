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
    public void ShouldDecodeProtoscopeBinaryFile(string filePath)
    {
        var data = File.ReadAllBytes(filePath);
        var decodeOptions = new ProtoDecodeOptions();
            
        var succeeded = ProtoObject.TryDecode(data, decodeOptions, out _);
            
        succeeded.ShouldEqual(true);
    }

    public static IEnumerable<object[]> GetProtoscopeBinaryFilePathsData()
    {
        return GetProtoscopeBinaryFilePaths().Select(x => new object[] { x });
    }

    private static string[] GetProtoscopeBinaryFilePaths()
    {
        var directoryPath = Path.Combine(AppContext.BaseDirectory, "Protoscope", "TestData");
        var filePaths = Directory.GetFiles(directoryPath, "*.pb");
        
        // These files contain unsupported features.
        var excludedFileNames = new[]
        {
            "groups.pb",
            "message.pb",
            "oneof.pb",
        };


        return filePaths.Where(x => !excludedFileNames.Contains(Path.GetFileName(x)))
                        .ToArray();
    }
}
