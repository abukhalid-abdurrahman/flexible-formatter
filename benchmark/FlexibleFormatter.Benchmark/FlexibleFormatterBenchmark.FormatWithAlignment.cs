using BenchmarkDotNet.Attributes;

namespace FlexibleFormatter.Benchmark;

public partial class FlexibleFormatterBenchmark
{
    private const string AlignmentTemplate = "[{0,10}] [{1,-10}] [{2,15:N2}]";
    private static readonly FlexibleFormatter _alignmentFormatter =
        FlexibleFormatter.Parse(format: AlignmentTemplate, style: ParameterStyle.Braces);

    [Benchmark]
    public string Alignment_MixedRightLeft_Formatting() =>
        _alignmentFormatter.Format("ABC", "XYZ", 1234.5678);
}
