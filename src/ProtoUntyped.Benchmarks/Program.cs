using BenchmarkDotNet.Running;

var benchmarkSwitcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
benchmarkSwitcher.Run();

Console.ReadLine();
