using BenchmarkDotNet.Attributes;

namespace FlexibleFormatter.Benchmark;

public partial class FlexibleFormatterBenchmark
{
    private static readonly string _templateContent =
        File.ReadAllText(Path.Combine("Templates", "order_confirmation_template_ru.html"));

    private static readonly FlexibleFormatter _dollarStyleFormatter =
        FlexibleFormatter.Parse(_templateContent, ParameterStyle.Dollar);

    private static readonly Dictionary<string, object?> _templateParameters = new()
    {
        ["OrderNumber"] = 123456,
        ["FullName"] = "John Doe",
        ["OrderDate"] = DateTime.Now.ToString("MMMM dd, yyyy"),
        ["ProductName"] = "UltraWidget Pro",
        ["Amount"] = 2,
        ["UnitPrice"] = 129.90,
        ["ShipmentMethod"] = "Стандартная — 3–5 дней",
        ["TotalAmount"] = 259.80,
        ["Address"] = "ул. Примерная, 10",
        ["EstimatedDate"] = DateTime.Now.AddDays(value: 5).ToString("MMMM dd, yyyy"),
    };

    [Benchmark]
    public string FlexibleFormatter_DollarStyle_Format() =>
        _dollarStyleFormatter.Format(_templateParameters);

    [Benchmark]
    public string StringReplace_DollarStyle_Template()
    {
        return _templateContent
            .Replace("$OrderNumber$", "123456", StringComparison.InvariantCulture)
            .Replace("$FullName$", "John Doe", StringComparison.InvariantCulture)
            .Replace("$OrderDate$", DateTime.Now.ToString("MMMM dd, yyyy"), StringComparison.InvariantCulture)
            .Replace("$ProductName$", "UltraWidget Pro", StringComparison.InvariantCulture)
            .Replace("$Amount$", "2", StringComparison.InvariantCulture)
            .Replace("$UnitPrice$", "129.90", StringComparison.InvariantCulture)
            .Replace("$ShipmentMethod$", "Стандартная — 3–5 дней", StringComparison.InvariantCulture)
            .Replace("$TotalAmount$", "259.80", StringComparison.InvariantCulture)
            .Replace("$Address$", "ул. Примерная, 10", StringComparison.InvariantCulture)
            .Replace("$EstimatedDate$", DateTime.Now.AddDays(value: 5).ToString("MMMM dd, yyyy"), StringComparison.InvariantCulture);
    }
}
