using BenchmarkDotNet.Attributes;

namespace FlexibleFormatter.Benchmark;

[MemoryDiagnoser]
public class FlexibleFormatterAllocatorBenchmark
{
    private const string Template2ParamsAllocator = "$First$ and $Second$";
    private static readonly FlexibleFormatter _formatterHeap2 =
        FlexibleFormatter.Parse(Template2ParamsAllocator, ParameterStyle.Dollar, AllocatorType.Heap);
    private static readonly FlexibleFormatter _formatterStack2 =
        FlexibleFormatter.Parse(Template2ParamsAllocator, ParameterStyle.Dollar, AllocatorType.StackAlloc);

    private const string Template4ParamsAllocator = "$P1$ $P2$ $P3$ $P4$";
    private static readonly FlexibleFormatter _formatterHeap4 =
        FlexibleFormatter.Parse(Template4ParamsAllocator, ParameterStyle.Dollar, AllocatorType.Heap);
    private static readonly FlexibleFormatter _formatterStack4 =
        FlexibleFormatter.Parse(Template4ParamsAllocator, ParameterStyle.Dollar, AllocatorType.StackAlloc);

    private const string Template7ParamsAllocator = "$P1$ $P2$ $P3$ $P4$ $P5$ $P6$ $P7$";
    private static readonly FlexibleFormatter _formatterHeap7 =
        FlexibleFormatter.Parse(Template7ParamsAllocator, ParameterStyle.Dollar, AllocatorType.Heap);
    private static readonly FlexibleFormatter _formatterStack7 =
        FlexibleFormatter.Parse(Template7ParamsAllocator, ParameterStyle.Dollar, AllocatorType.StackAlloc);

    [Benchmark(Description = "2 params - Dictionary (Heap)")]
    public string Format_2Params_Dictionary()
    {
        Dictionary<string, object?> args = new(capacity: 2)
        {
            ["First"] = "Hello",
            ["Second"] = "World"
        };
        return _formatterHeap2.Format(args);
    }

    [Benchmark(Description = "2 params - FormatNamed (Heap)")]
    public string Format_2Params_FormatNamed_Heap()
    {
        return _formatterHeap2.FormatNamed("First", "Hello", "Second", "World");
    }

    [Benchmark(Description = "2 params - FormatNamed (StackAlloc)")]
    public string Format_2Params_FormatNamed_StackAlloc()
    {
        return _formatterStack2.FormatNamed("First", "Hello", "Second", "World");
    }

    [Benchmark(Description = "4 params - Dictionary (Heap)")]
    public string Format_4Params_Dictionary()
    {
        Dictionary<string, object?> args = new(capacity: 4)
        {
            ["P1"] = "A",
            ["P2"] = "B",
            ["P3"] = "C",
            ["P4"] = "D"
        };
        return _formatterHeap4.Format(args);
    }

    [Benchmark(Description = "4 params - FormatNamed (Heap)")]
    public string Format_4Params_FormatNamed_Heap()
    {
        return _formatterHeap4.FormatNamed("P1", "A", "P2", "B", "P3", "C", "P4", "D");
    }

    [Benchmark(Description = "4 params - FormatNamed (StackAlloc)")]
    public string Format_4Params_FormatNamed_StackAlloc()
    {
        return _formatterStack4.FormatNamed("P1", "A", "P2", "B", "P3", "C", "P4", "D");
    }

    [Benchmark(Description = "7 params - Dictionary (Heap)")]
    public string Format_7Params_Dictionary()
    {
        Dictionary<string, object?> args = new(capacity: 7)
        {
            ["P1"] = "1",
            ["P2"] = "2",
            ["P3"] = "3",
            ["P4"] = "4",
            ["P5"] = "5",
            ["P6"] = "6",
            ["P7"] = "7"
        };
        return _formatterHeap7.Format(args);
    }

    [Benchmark(Description = "7 params - FormatNamed (Heap)")]
    public string Format_7Params_FormatNamed_Heap()
    {
        return _formatterHeap7.FormatNamed(
            "P1", "1",
            "P2", "2",
            "P3", "3",
            "P4", "4",
            "P5", "5",
            "P6", "6",
            "P7", "7");
    }

    [Benchmark(Description = "7 params - FormatNamed (StackAlloc)")]
    public string Format_7Params_FormatNamed_StackAlloc()
    {
        return _formatterStack7.FormatNamed(
            "P1", "1",
            "P2", "2",
            "P3", "3",
            "P4", "4",
            "P5", "5",
            "P6", "6",
            "P7", "7");
    }
}
