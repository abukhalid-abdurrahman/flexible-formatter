using BenchmarkDotNet.Attributes;

namespace FlexibleFormatter.Benchmark;

public partial class FlexibleFormatterBenchmark
{
    private const string LiteralsOnlyTemplate =
        @"This is a template with no parameters at all.
Just plain text that should be returned as-is.
It tests the fast path optimization for templates without any placeholders.
Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

    private static readonly FlexibleFormatter LiteralsOnlyFormatter =
        FlexibleFormatter.Parse(format: LiteralsOnlyTemplate, style: ParameterStyle.Braces);

    [Benchmark]
    public string LiteralsOnly_NoParameters_FastPath() =>
        LiteralsOnlyFormatter.Format();
}
