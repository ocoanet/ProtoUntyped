using BenchmarkDotNet.Attributes;
using ProtoUntyped.Benchmarks.Reference;

namespace ProtoUntyped.Benchmarks;

public class LargeMessageDecodingBenchmarks
{
    private readonly byte[] _data;

    public LargeMessageDecodingBenchmarks()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LargeMessage.txt");
        _data = Convert.FromBase64String(File.ReadAllText(filePath));
    }

    [Benchmark(Baseline = true)]
    public ProtoObject DecodeV1()
    {
        return ProtoObjectV1.Decode(_data);
    }
    
    [Benchmark]
    public ProtoObject DecodeV2()
    {
        return ProtoObject.Decode(_data);
    }
}
