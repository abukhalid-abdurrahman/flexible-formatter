using System.Text;
using BenchmarkDotNet.Attributes;

namespace FlexibleFormatter.Benchmark;

public partial class FlexibleFormatterBenchmark
{
    private static readonly string _manyParametersTemplate;
    private static readonly FlexibleFormatter _manyParametersFormatter;
    private static readonly Dictionary<string, object?> _manyParameters = [];

    static FlexibleFormatterBenchmark()
    {
        // Generate template with 50 parameters.
        StringBuilder sb = new();
        sb.AppendLine("Report:");
        for (int i = 0; i < 50; i++)
        {
            sb.AppendLine($"Field{i}: {{param{i}}}");
            _manyParameters[$"param{i}"] = $"Value{i}";
        }
        _manyParametersTemplate = sb.ToString();
        _manyParametersFormatter = FlexibleFormatter.Parse(
            format: _manyParametersTemplate,
            style: ParameterStyle.Braces);

        // Initialize Large HTML template.
        GenerateLargeHtmlTemplate();
    }

    [Benchmark]
    public string ManyParameters_50Fields_StressTest() =>
        _manyParametersFormatter.Format(_manyParameters);

    // ===== Large HTML with Dollar Style =====
    private static string _largeHtmlDollarTemplate = string.Empty;
    private static readonly Dictionary<string, object?> _largeHtmlParams = [];

    private static void GenerateLargeHtmlTemplate()
    {
        // Generate a realistic 50KB HTML template with $param$ style placeholders.
        StringBuilder sb = new();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("    <title>$Title$</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("        .header { background-color: #f0f0f0; padding: 10px; }");
        sb.AppendLine("        .content { margin-top: 20px; }");
        sb.AppendLine("        table { width: 100%; border-collapse: collapse; }");
        sb.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class='header'>");
        sb.AppendLine("        <h1>$CompanyName$</h1>");
        sb.AppendLine("        <p>Report Date: $ReportDate$</p>");
        sb.AppendLine("    </div>");
        sb.AppendLine("    <div class='content'>");
        sb.AppendLine("        <h2>Monthly Report for $Month$</h2>");
        sb.AppendLine("        <p>Dear $CustomerName$,</p>");
        sb.AppendLine("        <p>Here is your monthly summary:</p>");
        sb.AppendLine("        <table>");
        sb.AppendLine("            <tr><th>Metric</th><th>Value</th></tr>");

        // Add many rows with parameters to reach ~50KB.
        for (int i = 0; i < 200; i++)
        {
            sb.AppendLine($"            <tr><td>Metric {i}</td><td>$Metric{i}$</td></tr>");
            _largeHtmlParams[$"Metric{i}"] = $"Value_{i}";
        }

        sb.AppendLine("        </table>");
        sb.AppendLine("        <p>Total: $Total$</p>");
        sb.AppendLine("        <p>Average: $Average$</p>");
        sb.AppendLine("        <p>Comments: $Comments$</p>");
        sb.AppendLine("    </div>");
        sb.AppendLine("    <div class='footer'>");
        sb.AppendLine("        <p>Contact: $ContactEmail$ | Phone: $ContactPhone$</p>");
        sb.AppendLine("        <p>$Disclaimer$</p>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        _largeHtmlParams["Title"] = "Monthly Report";
        _largeHtmlParams["CompanyName"] = "ACME Corporation";
        _largeHtmlParams["ReportDate"] = DateTime.Now.ToString(format: "yyyy-MM-dd");
        _largeHtmlParams["Month"] = "November 2025";
        _largeHtmlParams["CustomerName"] = "John Smith";
        _largeHtmlParams["Total"] = "$125,430.50";
        _largeHtmlParams["Average"] = "$627.15";
        _largeHtmlParams["Comments"] = "Performance exceeded expectations this month.";
        _largeHtmlParams["ContactEmail"] = "support@acme.com";
        _largeHtmlParams["ContactPhone"] = "+1-555-0123";
        _largeHtmlParams["Disclaimer"] = "This is an automated report. All values are approximate.";

        _largeHtmlDollarTemplate = sb.ToString();
    }

    [Benchmark]
    public string LargeHtml_DollarStyle_200Parameters()
    {
        // Parse and format to simulate realistic usage.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: _largeHtmlDollarTemplate,
            style: ParameterStyle.Dollar);
        return formatter.Format(_largeHtmlParams);
    }

    [Benchmark(Baseline = true)]
    public string LargeHtml_StringReplace_200Parameters()
    {
        // Baseline: naive string.Replace approach for comparison.
        string result = _largeHtmlDollarTemplate;
        foreach (KeyValuePair<string, object?> param in _largeHtmlParams)
        {
            result = result.Replace(
                oldValue: $"${param.Key}$",
                newValue: param.Value?.ToString() ?? string.Empty, StringComparison.InvariantCulture);
        }
        return result;
    }
}
