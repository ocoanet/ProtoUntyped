using BenchmarkDotNet.Attributes;

namespace ProtoUntyped.Benchmarks;

public class LargeMessageDecodingBenchmarks
{
    private readonly byte[] _data;

    public LargeMessageDecodingBenchmarks()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LargeMessage.txt");
        _data = Convert.FromBase64String(File.ReadAllText(filePath));
    }

    [Benchmark]
    public ProtoObject DecodeV2()
    {
        return ProtoObject.Decode(_data);
    }
}
