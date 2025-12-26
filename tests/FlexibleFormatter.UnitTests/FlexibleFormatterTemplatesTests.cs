namespace FlexibleFormatter.UnitTests;

public class FlexibleFormatterTemplatesTests
{
    [Fact]
    public void Format_Generic_6Parameter_FormatsTemplateCorrectly()
    {
        // Arrange.
        string template =
            File.ReadAllText(Path.Combine("Templates", "SalaryChangeProcessedNotificationTemplateA.html"));

        FlexibleFormatter formatter = FlexibleFormatter.Parse(template, ParameterStyle.Dollar);

        // Act.
        string result = formatter.FormatNamed(
            "Receiver", "John Doe",
            "Date1", "December 2025",
            "DownloadUrl", "https://example.com/download/abc123",
            "DownloadStorageDays", 30,
            "ProxyFrontUrl", "https://proxy.example.com",
            "TestInfo", "<br><p>TEST EMPLOYEE NOTIFICATION: User Id: 12345</p>"
        );

        // Assert.
        Assert.Contains("John Doe", result, StringComparison.Ordinal);
        Assert.Contains("December 2025", result, StringComparison.Ordinal);
        Assert.Contains("https://example.com/download/abc123", result, StringComparison.Ordinal);
        Assert.Contains("30", result, StringComparison.Ordinal);
        Assert.Contains("https://proxy.example.com", result, StringComparison.Ordinal);
        Assert.Contains("<br><p>TEST EMPLOYEE NOTIFICATION: User Id: 12345</p>", result, StringComparison.Ordinal);
    }
}
