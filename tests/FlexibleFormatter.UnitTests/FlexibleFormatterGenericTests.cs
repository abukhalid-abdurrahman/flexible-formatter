namespace FlexibleFormatter.UnitTests;

/// <summary>
///     Unit tests for FlexibleFormatter - Generic Format methods (zero-allocation)
/// </summary>
public class FlexibleFormatterGenericTests
{
    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_1Parameter_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("Hello $Name$", ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed("Name", "World");

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_1Parameter_WithInteger_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("Count: $Count$", ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed("Count", 42);

        // Assert
        Assert.Equal("Count: 42", result);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_2Parameters_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("$First$ and $Second$", ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed("First", "Hello", "Second", "World");

        // Assert
        Assert.Equal("Hello and World", result);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_2Parameters_MixedTypes_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("Name: $Name$, Age: $Age$", ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed("Name", "Alice", "Age", 30);

        // Assert
        Assert.Equal("Name: Alice, Age: 30", result);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_3Parameters_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            "$A$, $B$, $C$",
            ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed(
            "A", "One",
            "B", "Two",
            "C", "Three");

        // Assert
        Assert.Equal("One, Two, Three", result);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_4Parameters_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            "$P1$ $P2$ $P3$ $P4$",
            ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed(
            "P1", "A",
            "P2", "B",
            "P3", "C",
            "P4", "D");

        // Assert
        Assert.Equal("A B C D", result);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_6Parameters_EmailNotification_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange - Email notification template
        string template = @"Dear $Receiver$, your document for $Date$ is ready.
Download: $DownloadUrl$
Valid for $Days$ days.
Proxy: $ProxyUrl$
$TestInfo$";

        FlexibleFormatter formatter = FlexibleFormatter.Parse(template, ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed(
            "Receiver", "John Doe",
            "Date", "December 2025",
            "DownloadUrl", "https://example.com/download",
            "Days", 30,
            "ProxyUrl", "https://proxy.example.com",
            "TestInfo", "Test Mode");

        // Assert
        Assert.Contains("John Doe", result, StringComparison.Ordinal);
        Assert.Contains("December 2025", result, StringComparison.Ordinal);
        Assert.Contains("https://example.com/download", result, StringComparison.Ordinal);
        Assert.Contains("30", result, StringComparison.Ordinal);
        Assert.Contains("https://proxy.example.com", result, StringComparison.Ordinal);
        Assert.Contains("Test Mode", result, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_7Parameters_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            "$P1$ $P2$ $P3$ $P4$ $P5$ $P6$ $P7$",
            ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed(
            "P1", "1",
            "P2", "2",
            "P3", "3",
            "P4", "4",
            "P5", "5",
            "P6", "6",
            "P7", "7");

        // Assert
        Assert.Equal("1 2 3 4 5 6 7", result);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_ProducesSameResultAs_Dictionary(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            "Hello $Name$, you have $Count$ messages",
            ParameterStyle.Dollar, allocatorType);

        // Act - Dictionary approach
        Dictionary<string, object?> dict = new(capacity: 2)
        {
            ["Name"] = "Alice",
            ["Count"] = 5
        };
        string resultDict = formatter.Format(dict);

        // Act - Generic approach
        string resultGeneric = formatter.FormatNamed("Name", "Alice", "Count", 5);

        // Assert
        Assert.Equal(resultDict, resultGeneric);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_6Params_ProducesSameResultAs_Dictionary(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            "$A$ $B$ $C$ $D$ $E$ $F$",
            ParameterStyle.Dollar, allocatorType);

        // Act - Dictionary approach
        Dictionary<string, object?> dict = new()
        {
            ["A"] = "1",
            ["B"] = "2",
            ["C"] = "3",
            ["D"] = "4",
            ["E"] = "5",
            ["F"] = "6"
        };
        string resultDict = formatter.Format(dict);

        // Act - Generic approach
        string resultGeneric = formatter.FormatNamed(
            "A", "1",
            "B", "2",
            "C", "3",
            "D", "4",
            "E", "5",
            "F", "6");

        // Assert
        Assert.Equal(resultDict, resultGeneric);
    }

    [Fact]
    public void Format_Generic_WithDateTime_FormatsCorrectly()
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("Date: $Date$", ParameterStyle.Dollar);
        DateTime testDate = new DateTime(2025, 12, 8, 10, 30, 0);

        // Act
        string result = formatter.FormatNamed("Date", testDate);

        // Assert
        Assert.Contains("2025", result, StringComparison.Ordinal);
        Assert.Contains("12", result, StringComparison.Ordinal);
        Assert.Contains("8", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_Generic_WithDecimal_FormatsCorrectly()
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("Price: $Price$", ParameterStyle.Dollar);

        // Act
        string result = formatter.FormatNamed("Price", 123.45m);

        // Assert
        Assert.Contains("123", result, StringComparison.Ordinal);
        Assert.Contains("45", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_Generic_WithBoolean_FormatsCorrectly()
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("Active: $Active$", ParameterStyle.Dollar);

        // Act
        string result = formatter.FormatNamed("Active", true);

        // Assert
        Assert.Equal("Active: True", result);
    }

    [Fact]
    public void Format_Generic_WithNull_FormatsAsEmpty()
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("Value: $Value$", ParameterStyle.Dollar);

        // Act
        string result = formatter.FormatNamed<string?>("Value", null);

        // Assert
        Assert.Equal("Value: ", result);
    }

    [Fact]
    public void Format_Generic_MissingParameter_ThrowsFormatException()
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("$A$ $B$", ParameterStyle.Dollar);

        // Act & Assert
        FormatException ex = Assert.Throws<FormatException>(() =>
            formatter.FormatNamed("A", "Value1", "WrongName", "Value2"));

        Assert.Contains("B", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_Generic_ParameterUsedMultipleTimes_FormatsCorrectly()
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("$Name$ likes $Name$", ParameterStyle.Dollar);

        // Act
        string result = formatter.FormatNamed("Name", "Alice");

        // Assert
        Assert.Equal("Alice likes Alice", result);
    }

    [Fact]
    public void Format_Generic_WithBraceStyle_FormatsCorrectly()
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("{Name} is {Age} years old", ParameterStyle.Braces);

        // Act
        string result = formatter.FormatNamed("Name", "Bob", "Age", 25);

        // Assert
        Assert.Equal("Bob is 25 years old", result);
    }

    [Fact]
    public void Format_Generic_WithPercentStyle_FormatsCorrectly()
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("%First% and %Second%", ParameterStyle.Percent);

        // Act
        string result = formatter.FormatNamed("First", "Hello", "Second", "World");

        // Assert
        Assert.Equal("Hello and World", result);
    }

    [Fact]
    public void Format_Generic_WithCustomObject_FormatsUsingToString()
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("User: $User$", ParameterStyle.Dollar);
        TestUser user = new()
        { Name = "John", Id = 123 };

        // Act
        string result = formatter.FormatNamed("User", user);

        // Assert
        Assert.Contains("John", result, StringComparison.Ordinal);
        Assert.Contains("123", result, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_7Parameters_WithMixedTypes_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            "$P1$ $P2$ $P3$ $P4$ $P5$ $P6$ $P7$",
            ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed(
            "P1", "text",
            "P2", 42,
            "P3", 3.14,
            "P4", true,
            "P5", 'X',
            "P6", 100L,
            "P7", "end");

        // Assert
        Assert.Contains("text", result, StringComparison.Ordinal);
        Assert.Contains("42", result, StringComparison.Ordinal);
        Assert.Contains("3", result, StringComparison.Ordinal); // Culture-independent check
        Assert.Contains("14", result, StringComparison.Ordinal); // Culture-independent check
        Assert.Contains("True", result, StringComparison.Ordinal);
        Assert.Contains("X", result, StringComparison.Ordinal);
        Assert.Contains("100", result, StringComparison.Ordinal);
        Assert.Contains("end", result, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_CanBeCalledMultipleTimes(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("$A$ $B$", ParameterStyle.Dollar, allocatorType);

        // Act - Multiple calls should work without issues
        string result1 = formatter.FormatNamed("A", "X", "B", "Y");
        string result2 = formatter.FormatNamed("A", "1", "B", "2");
        string result3 = formatter.FormatNamed("A", "Hello", "B", "World");

        // Assert
        Assert.Equal("X Y", result1);
        Assert.Equal("1 2", result2);
        Assert.Equal("Hello World", result3);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_WithEmptyString_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("Value: $Value$", ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed("Value", string.Empty);

        // Assert
        Assert.Equal("Value: ", result);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_WithWhitespace_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("'$Value$'", ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed("Value", "   ");

        // Assert
        Assert.Equal("'   '", result);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_WithSpecialCharacters_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse("Text: $Text$", ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed("Text", "Hello\nWorld\t!");

        // Assert
        Assert.Equal("Text: Hello\nWorld\t!", result);
    }

    [Theory]
    [InlineData(AllocatorType.Heap)]
    [InlineData(AllocatorType.StackAlloc)]
    public void Format_Generic_5Parameters_FormatsCorrectly(AllocatorType allocatorType)
    {
        // Arrange
        FlexibleFormatter formatter = FlexibleFormatter.Parse(
            "$P1$ $P2$ $P3$ $P4$ $P5$",
            ParameterStyle.Dollar, allocatorType);

        // Act
        string result = formatter.FormatNamed(
            "P1", "A",
            "P2", "B",
            "P3", "C",
            "P4", "D",
            "P5", "E");

        // Assert
        Assert.Equal("A B C D E", result);
    }

    private class TestUser
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }

        public override string ToString() => $"{Name} (ID: {Id})";
    }
}
