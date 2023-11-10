using BenchmarkDotNet.Attributes;

namespace KeenConveyance;

[SimpleJob]
[MemoryDiagnoser]
public class GenericBenchmark
{
    #region Public 属性

    [Params(2, 8)]
    public int Param1 { get; set; }

    #endregion Public 属性

    #region Public 方法

    [Benchmark]
    public void BenchmarkMethod()
    {
    }

    [IterationSetup]
    public void IterationSetup()
    {
    }

    #endregion Public 方法
}
