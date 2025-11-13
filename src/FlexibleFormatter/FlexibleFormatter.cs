using System.Diagnostics;
using System.Text;

namespace FlexibleFormatter;

/// <summary>
///     Represents a parsed flexible format string with support for multiple parameter styles.
/// </summary>
[DebuggerDisplay("{Template}")]
public sealed class FlexibleFormatter
{
    /// <summary>
    ///     The parsed segments that make up the format string.
    /// </summary>
    private readonly (string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format)[] _segments;

    /// <summary>
    ///     The sum of the lengths of all of the literals.
    /// </summary>
    private readonly int _literalLength;

    /// <summary>
    ///     The number of segments that represent format holes.
    /// </summary>
    private readonly int _formattedCount;

    /// <summary>
    ///     The number of args required to satisfy the format holes (for indexed parameters).
    /// </summary>
    private readonly int _argsRequired;

    private FlexibleFormatter(string format,
        (string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format)[] segments)
    {
        Debug.Assert(format is not null);
        Template = format;
        _segments = segments;

        // Compute derivative information from the segments
        int literalLength = 0, formattedCount = 0, argsRequired = 0;
        foreach ((string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format) segment in segments)
        {
            Debug.Assert((segment.Literal is not null) ^ (segment.ParamName is not null || segment.ArgIndex >= 0));

            if (segment.Literal is string literal)
            {
                literalLength += literal.Length;
            }
            else if (segment.ParamName is not null || segment.ArgIndex >= 0)
            {
                formattedCount++;
                if (segment.ArgIndex >= 0)
                {
                    argsRequired = Math.Max(argsRequired, segment.ArgIndex + 1);
                }
            }
        }

        _literalLength = literalLength;
        _formattedCount = formattedCount;
        _argsRequired = argsRequired;
    }

    /// <summary>
    ///     Parse the format string using the specified parameter style.
    /// </summary>
    public static FlexibleFormatter Parse(string format, ParameterStyle style = ParameterStyle.Braces)
    {
        ArgumentNullException.ThrowIfNull(format);

        return style switch
        {
            ParameterStyle.Braces => ParseBraces(format),
            ParameterStyle.Percent => ParseDelimited(format, '%', '%'),
            ParameterStyle.Dollar => ParseDelimited(format, '$', '$'),
            _ => throw new ArgumentException("Use ParseCustom for custom delimiters", nameof(style))
        };
    }

    /// <summary>
    ///     Parse the format string using custom delimiters.
    /// </summary>
    public static FlexibleFormatter ParseCustom(string format, string openDelimiter, string closeDelimiter)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(openDelimiter);
        ArgumentNullException.ThrowIfNull(closeDelimiter);

        if (openDelimiter.Length == 0 || closeDelimiter.Length == 0)
            throw new ArgumentException("Delimiters cannot be empty");

        return ParseDelimitedString(format, openDelimiter, closeDelimiter);
    }

    /// <summary>
    ///     Gets the original format string.
    /// </summary>
    public string Template { get; }

    /// <summary>
    ///     Gets the minimum number of arguments required (for indexed parameters).
    /// </summary>
    public int MinimumArgumentCount => _argsRequired;

    /// <summary>
    ///     Format using indexed arguments.
    /// </summary>
    public string Format(params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length < _argsRequired)
            throw new FormatException($"Format requires at least {_argsRequired} arguments, but only {args.Length} were provided");

        return FormatCore(args, null);
    }

    /// <summary>
    ///     Format using a dictionary of named parameters.
    /// </summary>
    public string Format(Dictionary<string, object?> namedArgs)
    {
        ArgumentNullException.ThrowIfNull(namedArgs);

        return FormatCore(null, namedArgs);
    }

    /// <summary>
    ///     Format using both indexed and named parameters.
    /// </summary>
    public string Format(object?[] indexedArgs, Dictionary<string, object?> namedArgs)
    {
        ArgumentNullException.ThrowIfNull(indexedArgs);
        ArgumentNullException.ThrowIfNull(namedArgs);

        if (indexedArgs.Length < _argsRequired)
            throw new FormatException($"Format requires at least {_argsRequired} arguments, but only {indexedArgs.Length} were provided");

        return FormatCore(indexedArgs, namedArgs);
    }

    /// <summary>Core formatting implementation.</summary>
    private string FormatCore(object?[]? indexedArgs, Dictionary<string, object?>? namedArgs)
    {
        // Calculate capacity: all literals + estimate for formatted values
        int capacity = _literalLength + _formattedCount * 8;
        StringBuilder sb = new(capacity);

        foreach ((string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format) segment in _segments)
        {
            if (segment.Literal is string literal)
            {
                // Append literal text
                sb.Append(literal);
            }
            else
            {
                // Get the value to format
                object? value;
                if (segment.ParamName != null)
                {
                    // Named parameter
                    if (namedArgs == null || !namedArgs.TryGetValue(segment.ParamName, out value))
                    {
                        throw new FormatException($"Named parameter '{segment.ParamName}' was not provided");
                    }
                }
                else
                {
                    // Indexed parameter
                    if (indexedArgs == null || segment.ArgIndex < 0 || segment.ArgIndex >= indexedArgs.Length)
                    {
                        throw new FormatException($"Index {segment.ArgIndex} is out of range");
                    }
                    value = indexedArgs[segment.ArgIndex];
                }

                // Format the value
                string formattedValue;
                if (segment.Format != null && value is IFormattable formattable)
                {
                    formattedValue = formattable.ToString(segment.Format, null);
                }
                else
                {
                    formattedValue = value?.ToString() ?? string.Empty;
                }

                // Apply alignment
                if (segment.Alignment != 0)
                {
                    int width = Math.Abs(segment.Alignment);
                    if (formattedValue.Length < width)
                    {
                        if (segment.Alignment > 0)
                        {
                            // Right align
                            sb.Append(' ', width - formattedValue.Length);
                            sb.Append(formattedValue);
                        }
                        else
                        {
                            // Left align
                            sb.Append(formattedValue);
                            sb.Append(' ', width - formattedValue.Length);
                        }
                    }
                    else
                    {
                        sb.Append(formattedValue);
                    }
                }
                else
                {
                    sb.Append(formattedValue);
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Parse format string with brace-style parameters: {0}, {name}, etc.
    /// </summary>
    private static FlexibleFormatter ParseBraces(string format)
    {
        List<(string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format)> segments = [];

        if (!TryParseBraceLiterals(format, segments, out int failureOffset, out string failureReason))
        {
            throw new FormatException($"{failureReason} at position {failureOffset}");
        }

        return new FlexibleFormatter(format, [.. segments]);
    }

    /// <summary>
    ///     Parse format string with single-character delimiters: %name%, $name$, etc.
    /// </summary>
    private static FlexibleFormatter ParseDelimited(string format, char open, char close)
    {
        List<(string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format)> segments = [];
        StringBuilder sb = new();

        int pos = 0;
        while (pos < format.Length)
        {
            int nextOpen = format.IndexOf(open, pos);

            if (nextOpen < 0)
            {
                // No more parameters, rest is literal
                sb.Append(format.AsSpan(pos));
                break;
            }

            // Append literal up to opening delimiter
            sb.Append(format.AsSpan(pos, nextOpen - pos));

            // Check for escaped delimiter (doubled)
            if (nextOpen + 1 < format.Length && format[nextOpen + 1] == open)
            {
                sb.Append(open);
                pos = nextOpen + 2;
                continue;
            }

            // Find closing delimiter
            int nextClose = format.IndexOf(close, nextOpen + 1);
            if (nextClose < 0)
            {
                throw new FormatException($"Unclosed parameter at position {nextOpen}");
            }

            // Save literal segment if any
            if (sb.Length > 0)
            {
                segments.Add((sb.ToString(), null, -1, 0, null));
                sb.Clear();
            }

            // Extract parameter name
            string? paramName = format.Substring(nextOpen + 1, nextClose - nextOpen - 1).Trim();
            if (string.IsNullOrEmpty(paramName))
            {
                throw new FormatException($"Empty parameter name at position {nextOpen}");
            }

            // Try to parse as integer index, otherwise treat as named parameter
            int index = -1;
            if (int.TryParse(paramName, out int parsedIndex))
            {
                index = parsedIndex;
                paramName = null;
            }

            segments.Add((null, paramName, index, 0, null));
            pos = nextClose + 1;
        }

        // Add final literal if any
        if (sb.Length > 0)
        {
            segments.Add((sb.ToString(), null, -1, 0, null));
        }

        return new FlexibleFormatter(format, segments.ToArray());
    }

    /// <summary>
    ///     Parse format string with multi-character custom delimiters.
    /// </summary>
    private static FlexibleFormatter ParseDelimitedString(string format, string open, string close)
    {
        List<(string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format)> segments = [];
        StringBuilder sb = new();

        int pos = 0;
        while (pos < format.Length)
        {
            int nextOpen = format.IndexOf(open, pos, StringComparison.Ordinal);

            if (nextOpen < 0)
            {
                sb.Append(format.AsSpan(pos));
                break;
            }

            sb.Append(format.AsSpan(pos, nextOpen - pos));

            // Check for escaped delimiter (doubled)
            if (nextOpen + open.Length * 2 <= format.Length &&
                format.Substring(nextOpen + open.Length, open.Length) == open)
            {
                sb.Append(open);
                pos = nextOpen + open.Length * 2;
                continue;
            }

            int nextClose = format.IndexOf(close, nextOpen + open.Length, StringComparison.Ordinal);
            if (nextClose < 0)
            {
                throw new FormatException($"Unclosed parameter at position {nextOpen}");
            }

            if (sb.Length > 0)
            {
                segments.Add((sb.ToString(), null, -1, 0, null));
                sb.Clear();
            }

            string? paramName = format.Substring(nextOpen + open.Length, nextClose - nextOpen - open.Length).Trim();
            if (string.IsNullOrEmpty(paramName))
            {
                throw new FormatException($"Empty parameter name at position {nextOpen}");
            }

            int index = -1;
            if (int.TryParse(paramName, out int parsedIndex))
            {
                index = parsedIndex;
                paramName = null;
            }

            segments.Add((null, paramName, index, 0, null));
            pos = nextClose + close.Length;
        }

        if (sb.Length > 0)
        {
            segments.Add((sb.ToString(), null, -1, 0, null));
        }

        return new FlexibleFormatter(format, segments.ToArray());
    }

    /// <summary>
    ///     Original brace-style parsing logic (adapted from CompositeFormat).
    /// </summary>
    private static bool TryParseBraceLiterals(string format,
        List<(string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format)> segments,
        out int failureOffset, out string failureReason)
    {
        StringBuilder sb = new();
        int pos = 0;
        char ch;

        failureOffset = 0;
        failureReason = string.Empty;

        while (true)
        {
            while (true)
            {
                // Find next brace
                int countUntilNextBrace = -1;
                for (int i = pos; i < format.Length; i++)
                {
                    if (format[i] == '{' || format[i] == '}')
                    {
                        countUntilNextBrace = i - pos;
                        break;
                    }
                }

                if (countUntilNextBrace < 0)
                {
                    sb.Append(format.AsSpan(pos));
                    segments.Add((sb.ToString(), null, -1, 0, null));
                    return true;
                }

                sb.Append(format.AsSpan(pos, countUntilNextBrace));
                pos += countUntilNextBrace;

                char brace = format[pos];
                if (!TryMoveNext(format, ref pos, out ch))
                {
                    failureReason = "Unclosed format item";
                    failureOffset = pos;
                    return false;
                }

                if (brace == ch)
                {
                    sb.Append(ch);
                    pos++;
                    continue;
                }

                if (brace != '{')
                {
                    failureReason = "Unexpected closing brace";
                    failureOffset = pos;
                    return false;
                }

                segments.Add((sb.ToString(), null, -1, 0, null));
                sb.Clear();
                break;
            }

            int width = 0;
            string? itemFormat = null;
            string? paramName = null;

            Debug.Assert(format[pos - 1] == '{');
            Debug.Assert(ch != '{');

            // Check if it's a named parameter (starts with letter/underscore)
            bool isNamed = char.IsLetter(ch) || ch == '_';
            int index = -1;

            if (isNamed)
            {
                // Parse named parameter
                int nameStart = pos;
                while (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    if (!TryMoveNext(format, ref pos, out ch))
                    {
                        failureReason = "Unclosed format item";
                        failureOffset = pos;
                        return false;
                    }
                }
                paramName = format.Substring(nameStart, pos - nameStart);
            }
            else
            {
                // Parse numeric index
                index = ch - '0';
                if ((uint)index >= 10u)
                {
                    failureReason = "Expected ASCII digit";
                    failureOffset = pos;
                    return false;
                }

                if (!TryMoveNext(format, ref pos, out ch))
                {
                    failureReason = "Unclosed format item";
                    failureOffset = pos;
                    return false;
                }

                if (ch != '}')
                {
                    while (char.IsDigit(ch))
                    {
                        index = index * 10 + ch - '0';
                        if (!TryMoveNext(format, ref pos, out ch))
                        {
                            failureReason = "Unclosed format item";
                            failureOffset = pos;
                            return false;
                        }
                    }

                    while (ch == ' ')
                    {
                        if (!TryMoveNext(format, ref pos, out ch))
                        {
                            failureReason = "Unclosed format item";
                            failureOffset = pos;
                            return false;
                        }
                    }

                    if (ch == ',')
                    {
                        do
                        {
                            if (!TryMoveNext(format, ref pos, out ch))
                            {
                                failureReason = "Unclosed format item";
                                failureOffset = pos;
                                return false;
                            }
                        }
                        while (ch == ' ');

                        int leftJustify = 1;
                        if (ch == '-')
                        {
                            leftJustify = -1;
                            if (!TryMoveNext(format, ref pos, out ch))
                            {
                                failureReason = "Unclosed format item";
                                failureOffset = pos;
                                return false;
                            }
                        }

                        width = ch - '0';
                        if ((uint)width >= 10u)
                        {
                            failureReason = "Expected ASCII digit";
                            failureOffset = pos;
                            return false;
                        }

                        if (!TryMoveNext(format, ref pos, out ch))
                        {
                            failureReason = "Unclosed format item";
                            failureOffset = pos;
                            return false;
                        }

                        while (char.IsDigit(ch))
                        {
                            width = width * 10 + ch - '0';
                            if (!TryMoveNext(format, ref pos, out ch))
                            {
                                failureReason = "Unclosed format item";
                                failureOffset = pos;
                                return false;
                            }
                        }
                        width *= leftJustify;

                        while (ch == ' ')
                        {
                            if (!TryMoveNext(format, ref pos, out ch))
                            {
                                failureReason = "Unclosed format item";
                                failureOffset = pos;
                                return false;
                            }
                        }
                    }

                    if (ch != '}')
                    {
                        if (ch != ':')
                        {
                            failureReason = "Unclosed format item";
                            failureOffset = pos;
                            return false;
                        }

                        int startingPos = pos;
                        while (true)
                        {
                            if (!TryMoveNext(format, ref pos, out ch))
                            {
                                failureReason = "Unclosed format item";
                                failureOffset = pos;
                                return false;
                            }

                            if (ch == '}')
                            {
                                break;
                            }

                            if (ch == '{')
                            {
                                failureReason = "Unclosed format item";
                                failureOffset = pos;
                                return false;
                            }
                        }

                        startingPos++;
                        itemFormat = format.Substring(startingPos, pos - startingPos);
                    }
                }
            }

            Debug.Assert(format[pos] == '}');
            pos++;

            segments.Add((null, paramName, index, width, itemFormat));
        }

        static bool TryMoveNext(string format, ref int pos, out char nextChar)
        {
            pos++;
            if ((uint)pos >= (uint)format.Length)
            {
                nextChar = '\0';
                return false;
            }

            nextChar = format[pos];
            return true;
        }
    }
}
