using BenchmarkDotNet.Attributes;

namespace FlexibleFormatter.Benchmark;

public partial class FlexibleFormatterBenchmark
{
    private const string SmallTemplate =
        @"Dear {name},

Thank you for your order {orderId}. We have received your payment of {amount}.
Your order will be shipped to {address} within {days} business days.

Best regards,
The Team";

    private static readonly FlexibleFormatter _smallFormatter =
        FlexibleFormatter.Parse(format: SmallTemplate, style: ParameterStyle.Braces);

    private static readonly Dictionary<string, object?> _smallParams = new()
    {
        ["name"] = "John Doe",
        ["orderId"] = "ORD-12345",
        ["amount"] = 99.99m,
        ["address"] = "123 Main St, City, State 12345",
        ["days"] = 5
    };

    [Benchmark]
    public string SmallTemplate_BracesStyle_NamedParameters() =>
        _smallFormatter.Format(_smallParams);
}
