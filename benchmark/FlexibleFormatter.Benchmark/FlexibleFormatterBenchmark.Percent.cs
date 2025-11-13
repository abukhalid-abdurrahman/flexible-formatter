using BenchmarkDotNet.Attributes;

namespace FlexibleFormatter.Benchmark;

public partial class FlexibleFormatterBenchmark
{
    private const string PercentTemplate = "User: %username%, Email: %email%, Status: %status%";
    private static readonly FlexibleFormatter _percentFormatter =
        FlexibleFormatter.Parse(format: PercentTemplate, style: ParameterStyle.Percent);

    private static readonly Dictionary<string, object?> _percentParams = new()
    {
        ["username"] = "alice_smith",
        ["email"] = "alice@example.com",
        ["status"] = "Active"
    };

    [Benchmark]
    public string PercentStyle_SimpleTemplate_NamedParameters() =>
        _percentFormatter.Format(_percentParams);
}
