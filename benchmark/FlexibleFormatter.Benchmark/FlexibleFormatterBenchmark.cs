
using BenchmarkDotNet.Attributes;

namespace FlexibleFormatter.Benchmark;

[MemoryDiagnoser]
public partial class FlexibleFormatterBenchmark
{
    private const string TemplateForStringFormat =
        "Dear {0}, your profile at {1} was last updated on {2}. You have {3} new messages and {4} pending tasks.";

    private static readonly FlexibleFormatter _flexibleFormatterIndexed =
        FlexibleFormatter.Parse(TemplateForStringFormat);

    private const  string _name = "Faridun Berdiev";
    private const  string _profileUrl = "https://wargaming.net/en";
    private static readonly DateTime _lastUpdated = DateTime.Now;
    private const int _newMessages = 12;
    private const int _pendingTasks = 3;

    [Benchmark]
    public string FlexibleFormatter_IndexedStyle_Format() =>
        _flexibleFormatterIndexed.Format(_name, _profileUrl, _lastUpdated.ToString("yyyy-MM-dd HH:mm"), _newMessages, _pendingTasks);

    [Benchmark]
    public string String_IndexedStyle_Format() =>
        string.Format(TemplateForStringFormat, _name, _profileUrl, _lastUpdated.ToString("yyyy-MM-dd HH:mm"), _newMessages, _pendingTasks);
}
