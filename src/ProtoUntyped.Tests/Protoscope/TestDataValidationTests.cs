using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ProtoUntyped.Tests.Protoscope;

/// <summary>
/// Validates ProtoUntyped using protoscope test data.
/// </summary>
public class TestDataValidationTests
{
    [Fact]
    public void ShouldFindProtoscopeBinaryFiles()
    {
        var filePaths = GetProtoscopeBinaryFilePaths();

        filePaths.Length.ShouldNotEqual(0);
    }

    /// <summary>
    /// Ensure ProtoUntyped can decode protoscope binary files.
    /// </summary>
    [Theory]
    [MemberData(nameof(DecodableProtoscopeBinaryFilesData))]
    public void ShouldDecodeProtoscopeBinaryFile(TestDataPath filePath)
    {
        var data = File.ReadAllBytes(filePath.Value);
        var decodeOptions = new ProtoDecodeOptions();

        var succeeded = ProtoObject.TryDecode(data, decodeOptions, out _);

        succeeded.ShouldEqual(true);
    }

    public static IEnumerable<object[]> DecodableProtoscopeBinaryFilesData
        => GetProtoscopeBinaryFilePaths().Select(x => new object[] { x });

    /// <summary>
    /// Ensure ProtoUntyped can generate protoscope output from binary files.
    /// </summary>
    /// <remarks>
    /// Only runs for binary files with matching <c>pb.golden.edit</c> files.
    /// </remarks>
    [Theory]
    [MemberData(nameof(ConvertibleProtoscopeBinaryFilesData))]
    public void ShouldGenerateProtoscopeString(TestDataPath filePath)
    {
        var data = File.ReadAllBytes(filePath.Value);
        var decodeOptions = new ProtoDecodeOptions();

        var obj = ProtoWireObject.Decode(data, decodeOptions);

        var expectedProtoscopeLines = File.ReadAllLines(filePath.ProtoscopeFilePath);
        var protoscopeLines = obj.ToProtoscopeString().TrimEnd().Split(Environment.NewLine);
        
        protoscopeLines.ShouldDeepEqual(expectedProtoscopeLines);
    }

    public static IEnumerable<object[]> ConvertibleProtoscopeBinaryFilesData
        => GetProtoscopeBinaryFilePaths().Where(x => x.HasProtoscopeFile()).Select(x => new object[] { x });

    [Fact]
    public void GenerateProtoscope()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Protoscope", "TestData", "message.pb");;
        var data = File.ReadAllBytes(filePath);
        var decodeOptions = new ProtoDecodeOptions();

        var obj = ProtoWireObject.Decode(data, decodeOptions);

        var outputFilePath = Path.ChangeExtension(filePath, "pb.golden.out");
        File.WriteAllText(outputFilePath, obj.ToProtoscopeString());
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

        public string ProtoscopeFilePath => Path.ChangeExtension(Value, "pb.golden.edit");

        public bool HasProtoscopeFile() => File.Exists(ProtoscopeFilePath);
    }
}