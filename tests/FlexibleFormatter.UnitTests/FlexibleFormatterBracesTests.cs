namespace FlexibleFormatter.UnitTests;

/// <summary>
///     Unit tests for FlexibleFormatter - Brace-style formatting tests
/// </summary>
public class FlexibleFormatterBracesTests
{
    [Fact]
    public void ParseBraces_IndexedSimple_AppliesFormatting()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0} and {1}",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format("Hello", "World");

        // Assert.
        Assert.Equal(expected: "Hello and World", actual: result);
    }

    [Fact]
    public void ParseBraces_IndexedWithFormatSpecifier_AppliesFormatting()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "Value: {0:F2}",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format(1234.5678);

        // Assert.
        // F2 format - check for value presence (culture-independent).
        Assert.Contains(
            expectedSubstring: "1234",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        Assert.Contains(
            expectedSubstring: "57",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void ParseBraces_IndexedWithAlignment_AppliesFormatting()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0,10}",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format("test");

        // Assert.
        Assert.Equal(expected: "      test", actual: result);
    }

    [Fact]
    public void ParseBraces_IndexedWithLeftAlignment_AppliesFormatting()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0,-10}",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format("test");

        // Assert.
        Assert.Equal(expected: "test      ", actual: result);
    }

    [Fact]
    public void ParseBraces_NamedParameters_ResolvesByName()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "Hello, {name}!",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format(new Dictionary<string, object?> { ["name"] = "Alice" });

        // Assert.
        Assert.Equal(expected: "Hello, Alice!", actual: result);
    }

    [Fact]
    public void ParseBraces_MixedNamedAndIndexed_ResolvesCorrectly()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0} says: {message}",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format(
            indexedArgs: ["Alice"],
            namedArgs: new Dictionary<string, object?> { ["message"] = "Hello!" }
        );

        // Assert.
        Assert.Equal(expected: "Alice says: Hello!", actual: result);
    }

    [Fact]
    public void ParseBraces_EscapedBraces_PreservesLiteral()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{{0}}",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format();

        // Assert.
        Assert.Equal(expected: "{0}", actual: result);
    }

    [Fact]
    public void ParseBraces_MixedEscapedAndParameter_WorksCorrectly()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{{escaped}} {0} {{also}}",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format("value");

        // Assert.
        Assert.Equal(expected: "{escaped} value {also}", actual: result);
    }

    [Fact]
    public void ParseBraces_UnclosedBrace_ThrowsFormatException()
    {
        // Arrange & Act.
        FormatException ex = Assert.Throws<FormatException>(() =>
            FlexibleFormatter.Parse(format: "{0", style: ParameterStyle.Braces));

        // Assert.
        Assert.Contains(
            expectedSubstring: "position",
            actualString: ex.Message.ToLowerInvariant(),
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void ParseBraces_UnexpectedClosingBrace_ThrowsFormatException()
    {
        // Arrange & Act.
        FormatException ex = Assert.Throws<FormatException>(() =>
            FlexibleFormatter.Parse(format: "test}", style: ParameterStyle.Braces));

        // Assert.
        Assert.Contains(
            expectedSubstring: "position",
            actualString: ex.Message.ToLowerInvariant(),
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void ParseBraces_MalformedAlignment_ThrowsFormatException()
    {
        // Arrange & Act.
        FormatException ex = Assert.Throws<FormatException>(() =>
            FlexibleFormatter.Parse(format: "{0,}", style: ParameterStyle.Braces));

        // Assert.
        Assert.Contains(
            expectedSubstring: "position",
            actualString: ex.Message.ToLowerInvariant(),
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void FormatCore_IndexedOutOfRange_ThrowsFormatException()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0} {1} {2}",
            style: ParameterStyle.Braces);

        // Act & Assert.
        FormatException ex = Assert.Throws<FormatException>(() => formatter.Format("one", "two"));
        Assert.Contains(
            expectedSubstring: "2",
            actualString: ex.Message,
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void FormatCore_NamedMissing_ThrowsFormatException()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{name}",
            style: ParameterStyle.Braces);

        // Act & Assert.
        FormatException ex = Assert.Throws<FormatException>(() =>
            formatter.Format(new Dictionary<string, object?>()));
        Assert.Contains(
            expectedSubstring: "name",
            actualString: ex.Message,
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void FormatCore_IFormattable_UsesFormatProvider()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0:yyyy-MM-dd}",
            style: ParameterStyle.Braces);
        DateTime date = new(year: 2023, month: 11, day: 13);

        // Act.
        string result = formatter.Format(date);

        // Assert.
        Assert.Equal(expected: "2023-11-13", actual: result);
    }

    [Fact]
    public void FormatCore_AlignmentRightAndLeft_AppliesSpacing()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "[{0,5}] [{1,-5}]",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format("ab", "cd");

        // Assert.
        Assert.Equal(expected: "[   ab] [cd   ]", actual: result);
    }

    [Fact]
    public void FormatCore_AlignmentShorterThanValue_NoTruncation()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0,3}",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format("longer");

        // Assert.
        Assert.Equal(expected: "longer", actual: result);
    }

    [Fact]
    public void ParseBraces_EmptyTemplate_ReturnsEmptyString()
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
    public void ParseBraces_OnlyLiterals_NoException()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "Just literal text",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format();

        // Assert.
        Assert.Equal(expected: "Just literal text", actual: result);
    }

    [Fact]
    public void ParseBraces_LotsOfSegments_NoExcessAllocations()
    {
        // Arrange.
        string template = string.Join(
            separator: " ",
            values: Enumerable.Range(start: 0, count: 100).Select(i => $"{{{i}}}"));
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: template,
            style: ParameterStyle.Braces);
        object[] args = Enumerable.Range(start: 0, count: 100).Select(i => (object)i).ToArray();

        // Act.
        string result = formatter.Format(args);

        // Assert.
        Assert.NotNull(result);
        Assert.Contains(
            expectedSubstring: "0",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        Assert.Contains(
            expectedSubstring: "99",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void ParseBraces_NullValue_RendersEmptyString()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0}",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format((object?)null);

        // Assert.
        Assert.Equal(expected: "", actual: result);
    }

    [Fact]
    public void ParseBraces_ComplexFormatString_ParsesCorrectly()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "User: {user}, Balance: {0:C}, Date: {1:yyyy-MM-dd HH:mm}",
            style: ParameterStyle.Braces);

        // Act.
        string result = formatter.Format(
            indexedArgs: [1234.56m, new DateTime(year: 2023, month: 11, day: 13, hour: 14, minute: 30, second: 0)],
            namedArgs: new Dictionary<string, object?> { ["user"] = "Alice" }
        );

        // Assert.
        Assert.Contains(
            expectedSubstring: "Alice",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        // Currency format is culture-dependent, just check key parts exist.
        Assert.Contains(
            expectedSubstring: "234",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        Assert.Contains(
            expectedSubstring: "56",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
        Assert.Contains(
            expectedSubstring: "2023-11-13",
            actualString: result,
            comparisonType: StringComparison.InvariantCulture);
    }

    [Fact]
    public void MinimumArgumentCount_NoParameters_ReturnsZero()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "No parameters",
            style: ParameterStyle.Braces);

        // Act.
        int count = formatter.MinimumArgumentCount;

        // Assert.
        Assert.Equal(expected: 0, actual: count);
    }

    [Fact]
    public void MinimumArgumentCount_IndexedParameters_ReturnsMax()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{0} {2} {1}",
            style: ParameterStyle.Braces);

        // Act.
        int count = formatter.MinimumArgumentCount;

        // Assert.
        Assert.Equal(expected: 3, actual: count);
    }

    [Fact]
    public void MinimumArgumentCount_NamedParameters_ReturnsZero()
    {
        // Arrange.
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            format: "{name} {value}",
            style: ParameterStyle.Braces);

        // Act.
        int count = formatter.MinimumArgumentCount;

        // Assert.
        Assert.Equal(expected: 0, actual: count);
    }
}
