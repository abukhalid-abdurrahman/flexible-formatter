namespace FlexibleFormatter.UnitTests;

/// <summary>
///     Unit tests for FlexibleFormatter - Edge cases and stress tests
/// </summary>
public class FlexibleFormatterEdgeCasesTests
{
    [Fact]
    public void EmptyTemplate_ReturnsEmptyString()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format();

        // Assert.
        Assert.Equal(expected: "", actual: result);
    }

    [Fact]
    public void OnlyLiterals_ReturnsLiterals()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "Just plain text",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format();

        // Assert.
        Assert.Equal(expected: "Just plain text", actual: result);
    }

    [Fact]
    public void NullArgument_ThrowsArgumentNullException()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0}",
            style: ParameterStyle.Braces);

        // Act & Assert.
        Assert.Throws<ArgumentNullException>(() => formatter.Format((object[])null!));
    }

    [Fact]
    public void NullDictionary_ThrowsArgumentNullException()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{name}",
            style: ParameterStyle.Braces);

        // Act & Assert.
        Assert.Throws<ArgumentNullException>(() => formatter.Format((Dictionary<string, object?>)null!));
    }

    [Fact]
    public void LargeTemplate_WithManyParameters_PerformsWell()
    {
        // Arrange.
        string template = string.Join(
            separator: " | ",
            values: Enumerable.Range(start: 0, count: 50).Select(i => $"{{{i}}}"));
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: template,
            style: ParameterStyle.Braces);
        object[] args = Enumerable.Range(start: 0, count: 50).Select(i => (object)$"val{i}").ToArray();

        // Act.
        string result = formatter.Format(args);

        // Assert.
        Assert.Contains(
            expectedSubstring: "val0",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        Assert.Contains(
            expectedSubstring: "val49",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        Assert.Contains(
            expectedSubstring: "|",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void Parse_NullFormat_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert.
        Assert.Throws<ArgumentNullException>(() =>
            FlexibleFormatter.Parse(format: null!, style: ParameterStyle.Braces));
    }

    [Fact]
    public void ParseCustom_NullDelimiters_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert.
        Assert.Throws<ArgumentNullException>(() =>
            FlexibleFormatter.ParseCustom(
                format: "test",
                openDelimiter: null!,
                closeDelimiter: ">>"));

        Assert.Throws<ArgumentNullException>(() =>
            FlexibleFormatter.ParseCustom(
                format: "test",
                openDelimiter: "<<",
                closeDelimiter: null!));
    }

    [Fact]
    public void Template_Property_ReturnsOriginalFormat()
    {
        // Arrange.
        const string template = "{0} and {name}";
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: template,
            style: ParameterStyle.Braces);

        // Act.
        string actualTemplate = formatter.Template;

        // Assert.
        Assert.Equal(expected: template, actual: actualTemplate);
    }

    [Fact]
    public void ComplexRealWorldScenario_EmailTemplate()
    {
        // Arrange.
        string template = @"Dear {name},

Your account balance is {0:C}.
Last login: {1:yyyy-MM-dd HH:mm}

You have {unread} unread messages.

Best regards,
{company}";

        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: template,
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format(
            indexedArgs: [1500.50m, new DateTime(year: 2023, month: 11, day: 13, hour: 9, minute: 30, second: 0)],
            namedArgs: new Dictionary<string, object?>
            {
                ["name"] = "Alice Johnson",
                ["unread"] = 5,
                ["company"] = "ACME Corp"
            }
        );

        // Assert.
        Assert.Contains(
            expectedSubstring: "Alice Johnson",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        // Currency format is culture-dependent, just check key parts exist.
        Assert.Contains(
            expectedSubstring: "500",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        Assert.Contains(
            expectedSubstring: "50",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        Assert.Contains(
            expectedSubstring: "2023-11-13",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        Assert.Contains(
            expectedSubstring: "5",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        Assert.Contains(
            expectedSubstring: "ACME Corp",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void FormatWithMixedArgs_OnlyIndexed_NamedIsNull()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0}",
            style: ParameterStyle.Braces);

        // Act.
        // This should work even though we're using the mixed signature.
        string result = formatter.Format(
            indexedArgs: ["test"],
            namedArgs: []);

        // Assert.
        Assert.Equal(expected: "test", actual: result);
    }
}
