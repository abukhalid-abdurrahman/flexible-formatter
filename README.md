# FlexibleFormatter

A high-performance, flexible string formatting library for .NET that supports multiple parameter styles including braces `{name}`, percent `%name%`, dollar `$name$`, and custom delimiters. Built "on top" of `CompositeFormatter`.

## Why Use FlexibleFormatter?

**Advantages over `string.Format`:**

- **Multiple parameter styles**: Choose the style that fits your use case (braces, percent, dollar, or custom)
- **Named parameters**: Use meaningful parameter names instead of numeric indices
- **Mixed parameters**: Combine indexed and named parameters in the same template
- **Parse once, format many**: Parse your template once and reuse it for multiple formatting operations
- **Performance**: Optimized for speed with minimal allocations
- **Clear error messages**: Get helpful error messages with exact positions when something goes wrong

## Quick Start

### Installation

```csharp
using FlexibleFormatter;
```

### Basic Usage

```csharp
// Parse a template with named and indexed parameters
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "Hello, {name}. Today is {0:yyyy-MM-dd}", 
    ParameterStyle.Braces);

// Format with both indexed and named arguments
string result = formatter.Format(
    new object[] { DateTime.UtcNow }, 
    new Dictionary<string, object?> { ["name"] = "Alice" });

// Output: "Hello, Alice. Today is 2025-11-13"
```

## Supported Styles

### 1. Braces Style (Default)

Standard composite format similar to `string.Format`. Supports indexed parameters, named parameters, alignment, and format specifiers.

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "User: {user}, Balance: {0:C}, Date: {1:yyyy-MM-dd HH:mm}", 
    ParameterStyle.Braces);

string result = formatter.Format(
    new object[] { 1234.56m, DateTime.Now },
    new Dictionary<string, object?> { ["user"] = "Bob" }
);
// Output: "User: Bob, Balance: $1,234.56, Date: 2025-11-13 14:30"
```

### 2. Percent Style

Uses `%name%` for parameters. Useful for configuration files and legacy systems.

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "Hello %name%! Your status is %status%.", 
    ParameterStyle.Percent);

string result = formatter.Format(new Dictionary<string, object?> 
{ 
    ["name"] = "Charlie",
    ["status"] = "Active"
});
// Output: "Hello Charlie! Your status is Active."
```

### 3. Dollar Style

Uses `$name$` for parameters. Common in shell scripts and certain template engines.

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "Order $orderId$ for $customer$", 
    ParameterStyle.Dollar);

string result = formatter.Format(new Dictionary<string, object?> 
{ 
    ["orderId"] = "12345",
    ["customer"] = "Alice"
});
// Output: "Order 12345 for Alice"
```

### 4. Custom Delimiters

Define your own opening and closing delimiters.

```csharp
FlexibleFormatter formatter = FlexibleFormatter.ParseCustom(
    "Hello << name >>, balance: << balance >> << currency >>", 
    "<<", ">>");

string result = formatter.Format(new Dictionary<string, object?> 
{ 
    ["name"] = "Dave",
    ["balance"] = "1,234.56",
    ["currency"] = "USD"
});
// Output: "Hello Dave, balance: 1,234.56 USD"
```

## Alignment & Format Specifiers

### Alignment

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "[{0,10}] [{1,-10}]", 
    ParameterStyle.Braces);

string result = formatter.Format("right", "left");
// Output: "[     right] [left      ]"
```

- Positive number: right-aligned
- Negative number: left-aligned
- Width smaller than value: no truncation

### Format Specifiers

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "Date: {0:yyyy-MM-dd}, Number: {1:F2}, Currency: {2:C}",
    ParameterStyle.Braces);

string result = formatter.Format(
    new DateTime(2025, 11, 13),
    1234.5678,
    99.99m);
// Output: "Date: 2025-11-13, Number: 1234.57, Currency: $99.99"
```

Supports all standard .NET format specifiers for types implementing `IFormattable`.

## Mixed Parameters

Combine indexed and named parameters in a single template:

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "Order {0} for {customer} contains {1} items. Status: {status}",
    ParameterStyle.Braces);

string result = formatter.Format(
    new object[] { "ORD-123", 5 },
    new Dictionary<string, object?> 
    { 
        ["customer"] = "Alice",
        ["status"] = "Shipped"
    }
);
// Output: "Order ORD-123 for Alice contains 5 items. Status: Shipped"
```

## Error Handling

FlexibleFormatter provides clear error messages with exact positions:

### Parsing Errors

```csharp
try
{
    FlexibleFormatter formatter = FlexibleFormatter.Parse("{0 unclosed", ParameterStyle.Braces);
}
catch (FormatException ex)
{
    // Message: "Unclosed format item at position 2"
}
```

### Formatting Errors

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse("{name}", ParameterStyle.Braces);

try
{
    string result = formatter.Format(new Dictionary<string, object?>());
}
catch (FormatException ex)
{
    // Message: "Named parameter 'name' was not provided"
}
```

### Out of Range Errors

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse("{0} {1} {2}", ParameterStyle.Braces);

try
{
    string result = formatter.Format("one", "two"); // Missing third argument
}
catch (FormatException ex)
{
    // Message: "Format requires at least 3 arguments, but only 2 were provided"
}
```

## Performance Tips

1. **Parse once, format many**: Parsing is more expensive than formatting, so reuse parsed formatters:

```csharp
// Good
FlexibleFormatter formatter = FlexibleFormatter.Parse(template, ParameterStyle.Braces);
for (int i = 0; i < 1000; i++)
{
    string result = formatter.Format(args);
}

// Bad (parses 1000 times)
for (int i = 0; i < 1000; i++)
{
    FlexibleFormatter formatter = FlexibleFormatter.Parse(template, ParameterStyle.Braces);
    string result = formatter.Format(args);
}
```

2. **Check MinimumArgumentCount**: Validate your arguments before formatting:

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse("{0} {1} {2}", ParameterStyle.Braces);
Console.WriteLine($"Requires at least {formatter.MinimumArgumentCount} arguments");
// Output: "Requires at least 3 arguments"
```

3. **Use appropriate parameter style**: Dollar and Percent styles are optimized for named parameters only.

## Examples

### Email Template

```csharp
string emailTemplate = @"Dear {name},

Your account balance is {0:C}.
Last login: {1:yyyy-MM-dd HH:mm}

You have {unread} unread messages.

Best regards,
{company}";

FlexibleFormatter formatter = FlexibleFormatter.Parse(emailTemplate, ParameterStyle.Braces);

string result = formatter.Format(
    new object[] { 1500.50m, new DateTime(2025, 11, 13, 9, 30, 0) },
    new Dictionary<string, object?> 
    { 
        ["name"] = "Alice Johnson",
        ["unread"] = 5,
        ["company"] = "ACME Corp"
    }
);
```

### HTML Template with Dollar Style

```csharp
string htmlTemplate = @"
<html>
    <body>
        <h1>$title$</h1>
        <p>Welcome, $username$!</p>
        <p>Last visit: $lastVisit$</p>
    </body>
</html>";

FlexibleFormatter formatter = FlexibleFormatter.Parse(htmlTemplate, ParameterStyle.Dollar);

string result = formatter.Format(new Dictionary<string, object?> 
{ 
    ["title"] = "Dashboard",
    ["username"] = "john_doe",
    ["lastVisit"] = DateTime.Now.ToString("yyyy-MM-dd")
});
```

## Benchmark Results

| Method                                         | Mean             | Error         | StdDev        | Ratio | Gen0     | Gen1    | Allocated | Alloc Ratio |
|----------------------------------------------- |-----------------:|--------------:|--------------:|------:|---------:|--------:|----------:|------------:|
| FlexibleFormatter_IndexedStyle_Format          |        186.20 ns |      1.854 ns |      1.734 ns | 0.000 |   0.1822 |       - |    1144 B |       0.000 |
| String_IndexedStyle_Format                     |        189.87 ns |      3.742 ns |      4.732 ns | 0.000 |   0.0777 |       - |     488 B |       0.000 |
| CustomDelimiters_AngleBrackets_NamedParameters |         71.60 ns |      0.717 ns |      0.671 ns | 0.000 |   0.0446 |       - |     280 B |       0.000 |
| FlexibleFormatter_DollarStyle_Format           |      2,911.50 ns |     57.918 ns |    118.311 ns | 0.000 |  10.6926 |  0.4845 |   67296 B |       0.012 |
| StringReplace_DollarStyle_Template             |  2,030,448.81 ns | 14,224.670 ns | 12,609.800 ns | 0.047 |  39.0625 |  3.9063 |  255712 B |       0.047 |
| Alignment_MixedRightLeft_Formatting            |        245.13 ns |      0.894 ns |      0.792 ns | 0.000 |   0.0787 |       - |     496 B |       0.000 |
| LiteralsOnly_NoParameters_FastPath             |         66.40 ns |      0.623 ns |      0.552 ns | 0.000 |   0.1606 |  0.0001 |    1008 B |       0.000 |
| PercentStyle_SimpleTemplate_NamedParameters    |        117.33 ns |      1.578 ns |      1.318 ns | 0.000 |   0.0789 |       - |     496 B |       0.000 |
| SmallTemplate_BracesStyle_NamedParameters      |        238.39 ns |      3.592 ns |      3.360 ns | 0.000 |   0.2232 |       - |    1400 B |       0.000 |
| ManyParameters_50Fields_StressTest             |        992.00 ns |     12.442 ns |     11.029 ns | 0.000 |   0.5684 |  0.0019 |    3576 B |       0.001 |
| LargeHtml_DollarStyle_200Parameters            |     18,109.17 ns |    341.758 ns |    302.959 ns | 0.000 |  23.8037 |  0.2136 |  149888 B |       0.028 |
| LargeHtml_StringReplace_200Parameters          | 43,292,280.27 ns | 93,447.559 ns | 82,838.827 ns | 1.000 | 833.3333 | 83.3333 | 5395429 B |       1.000 |
