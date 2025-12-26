# FlexibleFormatter

A high-performance, flexible string formatting library for .NET that supports multiple parameter styles including braces `{name}`, percent `%name%`, dollar `$name$`, and custom delimiters. Built "on top" of `CompositeFormatter`.

## Why Use FlexibleFormatter?

**Advantages over `string.Format`:**

- **Multiple parameter styles**: Choose the style that fits your use case (braces, percent, dollar, or custom)
- **Named parameters**: Use meaningful parameter names instead of numeric indices
- **Mixed parameters**: Combine indexed and named parameters in the same template
- **Parse once, format many**: Parse your template once and reuse it for multiple formatting operations
- **Performance**: Optimized for speed with minimal allocations
- **Memory strategies**: Choose between heap (`StringBuilder`) or stack allocation (`DefaultInterpolatedStringHandler`)
- **FormatNamed API**: Generic methods to avoid dictionary allocations for better performance
- **Configurable buffers**: Customize stack buffer size from 256 to 32,768 characters
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

## Memory Allocation Strategies

FlexibleFormatter supports two memory allocation strategies to optimize performance for different scenarios:

### AllocatorType.Heap (Default)

Uses `StringBuilder` for string construction. This is the default and safest option, suitable for strings of any size.

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "Hello {name}!", 
    ParameterStyle.Braces,
    AllocatorType.Heap);

string result = formatter.Format(new Dictionary<string, object?> { ["name"] = "Alice" });
```

### AllocatorType.StackAlloc

Uses `DefaultInterpolatedStringHandler` with stack-allocated buffers for maximum performance. Ideal for hot paths and frequently called formatting operations.

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "Hello {name}!", 
    ParameterStyle.Braces,
    AllocatorType.StackAlloc);

string result = formatter.Format(new Dictionary<string, object?> { ["name"] = "Alice" });
```

**Benefits:**
- Significantly faster for small to medium-sized strings
- Reduces GC pressure by avoiding heap allocations
- Perfect for high-throughput scenarios

**Limitations:**
- Buffer size is limited (default: 256 characters, max: 32,768 characters)
- Not suitable for very large templates or formatted output

### Custom Buffer Size

When using `StackAlloc`, you can specify a custom buffer size between 0 and 32,768 characters:

```csharp
// For larger templates, increase buffer size
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    largeEmailTemplate, 
    ParameterStyle.Dollar,
    AllocatorType.StackAlloc,
    stackAllocBufferSize: 8192);  // 8KB buffer
```

**Buffer Size Guidelines:**
- **Small templates (< 256 chars)**: Use default size (256)
- **Medium templates (256-2048 chars)**: Use 1024-2048
- **Large templates (2048-8192 chars)**: Use 4096-8192
- **Very large templates (> 8192 chars)**: Use up to 32,768 or consider `AllocatorType.Heap`

### Choosing the Right Strategy

| Scenario | Recommended AllocatorType | Buffer Size |
|----------|--------------------------|-------------|
| Small, frequently formatted strings | StackAlloc | 256 (default) |
| Email templates (< 2KB) | StackAlloc | 1024-2048 |
| Large HTML templates (2-8KB) | StackAlloc | 4096-8192 |
| Very large templates (> 8KB) | StackAlloc | Up to 32,768 |
| Unknown/variable size output | Heap | N/A |
| Low-frequency formatting | Heap | N/A |

## FormatNamed API

For better performance with named parameters, use the generic `FormatNamed` methods to avoid dictionary allocations:

### Basic Usage

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "Hello {firstName} {lastName}!", 
    ParameterStyle.Braces);

// Instead of using a dictionary
string result = formatter.FormatNamed(
    "firstName", "John",
    "lastName", "Doe");
// Output: "Hello John Doe!"
```

### Performance Comparison

```csharp
// With Dictionary (slower, allocates dictionary)
string result1 = formatter.Format(new Dictionary<string, object?> 
{ 
    ["firstName"] = "John",
    ["lastName"] = "Doe"
});

// With FormatNamed (faster, no dictionary allocation)
string result2 = formatter.FormatNamed(
    "firstName", "John",
    "lastName", "Doe");
```

### Supported Overloads

`FormatNamed` supports 1 to 7 named parameters:

```csharp
// 1 parameter
formatter.FormatNamed("name", "Alice");

// 2 parameters
formatter.FormatNamed(
    "firstName", "John",
    "lastName", "Doe");

// 3 parameters
formatter.FormatNamed(
    "title", "Mr.",
    "firstName", "John",
    "lastName", "Doe");

// ... up to 7 parameters
formatter.FormatNamed(
    "param1", value1,
    "param2", value2,
    "param3", value3,
    "param4", value4,
    "param5", value5,
    "param6", value6,
    "param7", value7);
```

### Real-World Example

```csharp
// Email notification template
FlexibleFormatter emailFormatter = FlexibleFormatter.Parse(
    "Dear {title} {lastName}, your order #{orderId} has been shipped to {city}, {country}.",
    ParameterStyle.Braces,
    AllocatorType.StackAlloc,
    stackAllocBufferSize: 512);

// High-performance formatting without dictionary allocation
string notification = emailFormatter.FormatNamed(
    "title", "Mr.",
    "lastName", "Smith",
    "orderId", "ORD-2025-12345",
    "city", "New York",
    "country", "USA");
// Output: "Dear Mr. Smith, your order #ORD-2025-12345 has been shipped to New York, USA."
```

**Benefits of FormatNamed:**
- No dictionary allocation overhead
- Type-safe at compile time with generic parameters
- Faster execution for 1-7 parameters
- Works seamlessly with both `Heap` and `StackAlloc` allocators

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

2. **Use StackAlloc for hot paths**: For frequently called formatting operations, use `AllocatorType.StackAlloc`:

```csharp
// High-performance formatter with stack allocation
FlexibleFormatter formatter = FlexibleFormatter.Parse(
    "Request {id}: {method} {path} - {status}",
    ParameterStyle.Braces,
    AllocatorType.StackAlloc,
    stackAllocBufferSize: 512);

// In hot path (called millions of times)
string logEntry = formatter.Format(requestId, method, path, statusCode);
```

3. **Use FormatNamed for named parameters**: Avoid dictionary allocations with the generic `FormatNamed` API:

```csharp
// Slower: allocates dictionary
string result = formatter.Format(new Dictionary<string, object?> { ["name"] = "Alice" });

// Faster: no dictionary allocation
string result = formatter.FormatNamed("name", "Alice");
```

4. **Check MinimumArgumentCount**: Validate your arguments before formatting:

```csharp
FlexibleFormatter formatter = FlexibleFormatter.Parse("{0} {1} {2}", ParameterStyle.Braces);
Console.WriteLine($"Requires at least {formatter.MinimumArgumentCount} arguments");
// Output: "Requires at least 3 arguments"
```

5. **Choose appropriate buffer size**: Match buffer size to your expected output:

```csharp
// Small templates: default (256)
var small = FlexibleFormatter.Parse(template, style, AllocatorType.StackAlloc);

// Large templates: custom size
var large = FlexibleFormatter.Parse(template, style, AllocatorType.StackAlloc, 4096);
```

6. **Use appropriate parameter style**: Dollar and Percent styles are optimized for named parameters only.

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

| Method                                         | Mean             | Error          | StdDev         | Median           | Ratio | Gen0     | Gen1   | Allocated | Alloc Ratio |
|----------------------------------------------- |-----------------:|---------------:|---------------:|-----------------:|------:|---------:|-------:|----------:|------------:|
| FlexibleFormatter_IndexedStyle_Format          |        139.52 ns |       2.799 ns |       4.599 ns |        137.48 ns | 0.000 |   0.0682 |      - |    1144 B |       0.000 |
| String_IndexedStyle_Format                     |        153.57 ns |       0.628 ns |       0.557 ns |        153.53 ns | 0.000 |   0.0291 |      - |     488 B |       0.000 |
| CustomDelimiters_AngleBrackets_NamedParameters |         56.32 ns |       0.609 ns |       0.509 ns |         56.21 ns | 0.000 |   0.0167 |      - |     280 B |       0.000 |
| FlexibleFormatter_DollarStyle_Format           |      1,196.40 ns |      26.082 ns |      75.667 ns |      1,150.73 ns | 0.000 |   2.5997 | 0.0992 |   43576 B |       0.008 |
| StringReplace_DollarStyle_Template             |    771,102.50 ns |   2,499.107 ns |   2,215.393 ns |    770,243.75 ns | 0.020 |   5.8594 |      - |  109016 B |       0.020 |
| Alignment_MixedRightLeft_Formatting            |        149.99 ns |       0.975 ns |       0.814 ns |        149.98 ns | 0.000 |   0.0296 |      - |     496 B |       0.000 |
| LiteralsOnly_NoParameters_FastPath             |         40.69 ns |       0.562 ns |       0.498 ns |         40.79 ns | 0.000 |   0.0603 | 0.0001 |    1008 B |       0.000 |
| PercentStyle_SimpleTemplate_NamedParameters    |         72.58 ns |       0.424 ns |       0.376 ns |         72.48 ns | 0.000 |   0.0296 |      - |     496 B |       0.000 |
| SmallTemplate_BracesStyle_NamedParameters      |        153.20 ns |       1.254 ns |       1.173 ns |        153.05 ns | 0.000 |   0.0837 |      - |    1400 B |       0.000 |
| ManyParameters_50Fields_StressTest             |        813.25 ns |       6.277 ns |       4.901 ns |        812.61 ns | 0.000 |   0.2251 |      - |    3776 B |       0.001 |
| LargeHtml_DollarStyle_200Parameters            |     13,304.48 ns |     265.824 ns |     248.652 ns |     13,385.26 ns | 0.000 |   9.0485 | 1.9379 |  151648 B |       0.028 |
| LargeHtml_StringReplace_200Parameters          | 39,166,810.99 ns | 161,735.526 ns | 143,374.331 ns | 39,138,092.31 ns | 1.000 | 307.6923 |      - | 5491432 B |       1.000 |
