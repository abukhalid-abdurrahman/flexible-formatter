namespace FlexibleFormatter.UnitTests;

/// <summary>
///     Unit tests for FlexibleFormatter - Delimited style (Percent and Dollar) tests
/// </summary>
public class FlexibleFormatterDelimitedTests
{
    [Fact]
    public void ParseDelimited_Percent_SimpleNamed_ReplacesValue()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "Hello %name%!",
            style: ParameterStyle.Percent);

        // Act.
        string result = formatter.Format(new Dictionary<string, object?> { ["name"] = "Bob" });

        // Assert.
        Assert.Equal(expected: "Hello Bob!", actual: result);
    }

    [Fact]
    public void ParseDelimited_Percent_EscapedPercent_PreservesPercent()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "%%name%%",
            style: ParameterStyle.Percent);

        // Act.
        string result = formatter.Format();

        // Assert.
        Assert.Equal(expected: "%name%", actual: result);
    }

    [Fact]
    public void ParseDelimited_Percent_SimpleIndexed_ReplacesValue()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "Value: %0%",
            style: ParameterStyle.Percent);

        // Act.
        string result = formatter.Format(123);

        // Assert.
        Assert.Equal(expected: "Value: 123", actual: result);
    }

    [Fact]
    public void ParseDelimited_Percent_Unclosed_ThrowsFormatException()
    {
        // Arrange & Act.
        FormatException ex = Assert.Throws<FormatException>(() =>
            FlexibleFormatter.Parse(format: "Hello %name", style: ParameterStyle.Percent));

        // Assert.
        Assert.Contains(
            expectedSubstring: "position",
            actualString: ex.Message.ToLowerInvariant(),
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void ParseDelimited_Percent_EmptyParameterName_ThrowsFormatException()
    {
        // Arrange & Act.
        FormatException ex = Assert.Throws<FormatException>(() =>
            FlexibleFormatter.Parse(format: "Hello % %!", style: ParameterStyle.Percent));

        // Assert.
        Assert.Contains(
            expectedSubstring: "Empty parameter name",
            actualString: ex.Message,
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void ParseDelimited_Percent_MultipleParameters_WorksCorrectly()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "%first% and %second%",
            style: ParameterStyle.Percent);

        // Act.
        string result = formatter.Format(new Dictionary<string, object?>
        {
            ["first"] = "A",
            ["second"] = "B"
        });

        // Assert.
        Assert.Equal(expected: "A and B", actual: result);
    }

    [Fact]
    public void ParseDelimited_Dollar_SimpleNamed_ReplacesValue()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "Hello $name$!",
            style: ParameterStyle.Dollar);

        // Act.
        string result = formatter.Format(new Dictionary<string, object?> { ["name"] = "Charlie" });

        // Assert.
        Assert.Equal(expected: "Hello Charlie!", actual: result);
    }

    [Fact]
    public void ParseDelimited_Dollar_EscapedDollar_PreservesDollar()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "$$price$$",
            style: ParameterStyle.Dollar);

        // Act.
        string result = formatter.Format();

        // Assert.
        Assert.Equal(expected: "$price$", actual: result);
    }

    [Fact]
    public void ParseDelimited_Dollar_SimpleIndexed_ReplacesValue()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "Item $0$",
            style: ParameterStyle.Dollar);

        // Act.
        string result = formatter.Format("first");

        // Assert.
        Assert.Equal(expected: "Item first", actual: result);
    }

    [Fact]
    public void ParseDelimited_Dollar_Unclosed_ThrowsFormatException()
    {
        // Arrange & Act.
        FormatException ex = Assert.Throws<FormatException>(() =>
            FlexibleFormatter.Parse(format: "Value $name", style: ParameterStyle.Dollar));

        // Assert.
        Assert.Contains(
            expectedSubstring: "position",
            actualString: ex.Message.ToLowerInvariant(),
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void ParseDelimited_Dollar_WithWhitespace_TrimsAndResolves()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "$ name $",
            style: ParameterStyle.Dollar);

        // Act.
        string result = formatter.Format(new Dictionary<string, object?> { ["name"] = "value" });

        // Assert.
        Assert.Equal(expected: "value", actual: result);
    }
}
