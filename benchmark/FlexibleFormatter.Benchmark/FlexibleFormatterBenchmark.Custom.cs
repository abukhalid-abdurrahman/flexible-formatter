using BenchmarkDotNet.Attributes;

namespace FlexibleFormatter.Benchmark;

public partial class FlexibleFormatterBenchmark
{
    private const string CustomTemplate = "Hello << name >>, your balance is << balance >> << currency >>.";
    private static readonly FlexibleFormatter _customFormatter =
        FlexibleFormatter.ParseCustom(
            format: CustomTemplate,
            openDelimiter: "<<",
            closeDelimiter: ">>");

    private static readonly Dictionary<string, object?> CustomParams = new()
    {
        ["name"] = "Bob",
        ["balance"] = "1,234.56",
        ["currency"] = "USD"
    };

    [Benchmark]
    public string CustomDelimiters_AngleBrackets_NamedParameters() =>
        _customFormatter.Format(CustomParams);
}
