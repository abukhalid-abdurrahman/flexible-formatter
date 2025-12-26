# CHANGELOG

## [1.0.0] - 2025-11-13

### Added

#### Core Features
- **Multiple Parameter Styles**: Support for four different formatting styles:
  - Braces style `{name}` - Standard composite format similar to `string.Format`
  - Percent style `%name%` - Useful for configuration files and legacy systems
  - Dollar style `$name$` - Common in shell scripts and template engines
  - Custom delimiters - Define your own opening and closing delimiters
- **Named Parameters**: Use meaningful parameter names instead of numeric indices in templates
- **Indexed Parameters**: Traditional numeric indexing for backward compatibility
- **Mixed Parameters**: Combine both indexed and named parameters in the same template
- **Alignment Support**: Control text alignment with configurable padding in braces style
  - Left alignment: `{0,-10}`
  - Right alignment: `{0,10}`
- **Format Specifiers**: Apply custom formatting with format strings in braces style
  - Example: `{0:yyyy-MM-dd}`, `{value:C}`, `{amount:N2}`
- **Parse Once, Format Many**: Parse templates once and reuse them for multiple formatting operations
- **Flexible Formatting Methods**:
  - `Format(params object?[] args)` - Format with indexed arguments
  - `Format(Dictionary<string, object?> namedArgs)` - Format with named parameters
  - `Format(object?[] indexedArgs, Dictionary<string, object?> namedArgs)` - Format with both

#### Parsing Features
- **Multiple Parsing Methods**:
  - `Parse(string format, ParameterStyle style)` - Parse with predefined styles
  - `ParseCustom(string format, string openDelimiter, string closeDelimiter)` - Parse with custom delimiters
- **Robust Error Handling**: Clear error messages with exact error positions
- **Escape Sequences**: Support for escaped delimiters
  - Braces: `{{` and `}}`
  - Single-char delimiters: Doubled delimiters (e.g., `%%`, `$$`)
  - Multi-char delimiters: Doubled delimiters
- **Template Information**: Access to parsed template and minimum argument count via public properties

#### Performance Features
- **Optimized String Building**: Efficient `StringBuilder` implementation with pre-calculated capacity
- **Minimal Allocations**: Design focused on reducing memory allocations
- **Literal Length Caching**: Pre-computed literal length for better performance
- **Formatted Count Tracking**: Optimized capacity planning based on format hole count

### Public API
- `FlexibleFormatter` class - Main formatter class
- `ParameterStyle` enum - Defines parameter style options (Braces, Percent, Dollar)
- Properties:
  - `Template` - Gets the original format string
  - `MinimumArgumentCount` - Gets the minimum number of arguments required for indexed parameters

## [2.0.0] - 2025-12-26

### Added

- Updating to **.NET 10**
