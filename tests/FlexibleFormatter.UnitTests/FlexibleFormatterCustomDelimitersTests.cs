namespace FlexibleFormatter.UnitTests;

/// <summary>
///     Unit tests for FlexibleFormatter - Custom delimiter tests
/// </summary>
public class FlexibleFormatterCustomDelimitersTests
{
    [Fact]
    public void ParseCustom_MultiCharDelimiters_ReplacesValue()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.ParseCustom(
            format: "Hello <% name %>!",
            openDelimiter: "<%",
            closeDelimiter: "%>");

        // Act.
        string result = formatter.Format(new Dictionary<string, object?> { ["name"] = "Dave" });

        // Assert.
        Assert.Equal(expected: "Hello Dave!", actual: result);
    }

    [Fact]
    public void ParseCustom_EscapedDelimiter_PreservesLiteral()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.ParseCustom(
            format: "<%<% text <% name %>",
            openDelimiter: "<%",
            closeDelimiter: "%>");

        // Act.
        string result = formatter.Format(new Dictionary<string, object?> { ["name"] = "test" });

        // Assert.
        Assert.Equal(expected: "<% text test", actual: result);
    }

    [Fact]
    public void ParseCustom_DoubleAngleBrackets_ReplacesValue()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.ParseCustom(
            format: "Value: << var >>",
            openDelimiter: "<<",
            closeDelimiter: ">>");

        // Act.
        string result = formatter.Format(new Dictionary<string, object?> { ["var"] = "42" });

        // Assert.
        Assert.Equal(expected: "Value: 42", actual: result);
    }

    [Fact]
    public void ParseCustom_EmptyDelimiters_ThrowsArgumentException()
    {
        // Arrange, Act & Assert.
        Assert.Throws<ArgumentException>(() =>
            FlexibleFormatter.ParseCustom(
                format: "test",
                openDelimiter: "",
                closeDelimiter: ">>"));

        Assert.Throws<ArgumentException>(() =>
            FlexibleFormatter.ParseCustom(
                format: "test",
                openDelimiter: "<<",
                closeDelimiter: ""));
    }

    [Fact]
    public void ParseCustom_UnclosedParameter_ThrowsFormatException()
    {
        // Arrange & Act.
        FormatException ex = Assert.Throws<FormatException>(() =>
            FlexibleFormatter.ParseCustom(
                format: "Hello << name",
                openDelimiter: "<<",
                closeDelimiter: ">>"));

        // Assert.
        Assert.Contains(
            expectedSubstring: "position",
            actualString: ex.Message.ToLowerInvariant(),
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void ParseCustom_IndexedParameter_WorksCorrectly()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.ParseCustom(
            format: "<< 0 >> and << 1 >>",
            openDelimiter: "<<",
            closeDelimiter: ">>");

        // Act.
        string result = formatter.Format("first", "second");

        // Assert.
        Assert.Equal(expected: "first and second", actual: result);
    }
}
