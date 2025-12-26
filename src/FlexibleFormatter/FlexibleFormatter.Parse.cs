using System.Diagnostics;
using System.Text;
using FlexibleFormatter.Resources;

namespace FlexibleFormatter;

public sealed partial class FlexibleFormatter
{
    /// <summary>
    ///     Parse the format string using the specified parameter style.
    /// </summary>
    /// <param name="format">The format string to parse.</param>
    /// <param name="style">The parameter style to use.</param>
    /// <param name="allocatorType">The memory allocation strategy.</param>
    /// <param name="stackAllocBufferSize">
    ///     Buffer size for stack allocation (used when allocatorType is StackAlloc). 
    ///     Must be between 0 and 32768. Default is 256 characters.
    /// </param>
    public static FlexibleFormatter Parse(string format, ParameterStyle style = ParameterStyle.Braces, AllocatorType allocatorType = AllocatorType.Heap, int stackAllocBufferSize = DefaultStackAllocBufferSize)
    {
        ArgumentNullException.ThrowIfNull(format);

        ValidateBufferSize(stackAllocBufferSize);

        return style switch
        {
            ParameterStyle.Braces => ParseBraces(format, allocatorType, stackAllocBufferSize),
            ParameterStyle.Percent => ParseDelimited(format, '%', '%', allocatorType, stackAllocBufferSize),
            ParameterStyle.Dollar => ParseDelimited(format, '$', '$', allocatorType, stackAllocBufferSize),
            _ => throw new ArgumentException(ExceptionMessages.UseParseCustomForCustomDelimiters, nameof(style))
        };
    }

    /// <summary>
    ///     Parse the format string using custom delimiters.
    /// </summary>
    /// <param name="format">The format string to parse.</param>
    /// <param name="openDelimiter">The opening delimiter.</param>
    /// <param name="closeDelimiter">The closing delimiter.</param>
    /// <param name="allocatorType">The memory allocation strategy.</param>
    /// <param name="stackAllocBufferSize">
    ///     Buffer size for stack allocation (used when allocatorType is StackAlloc). 
    ///     Must be between 0 and 32768. Default is 256 characters.
    /// </param>
    public static FlexibleFormatter ParseCustom(string format, string openDelimiter, string closeDelimiter, AllocatorType allocatorType = AllocatorType.Heap, int stackAllocBufferSize = DefaultStackAllocBufferSize)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(openDelimiter);
        ArgumentNullException.ThrowIfNull(closeDelimiter);

        ValidateBufferSize(stackAllocBufferSize);

        if (openDelimiter.Length is 0 || closeDelimiter.Length is 0)
            throw new ArgumentException(ExceptionMessages.DelimitersCannotBeEmpty);

        return ParseDelimitedString(format, openDelimiter, closeDelimiter, allocatorType, stackAllocBufferSize);
    }

    /// <summary>
    ///     Parse format string with brace-style parameters: {0}, {name}, etc.
    /// </summary>
    private static FlexibleFormatter ParseBraces(string format, AllocatorType allocatorType, int stackAllocBufferSize)
    {
        List<(string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format)> segments = [];

        if (!TryParseBraceLiterals(format, segments, out int failureOffset, out string failureReason))
        {
            throw new FormatException($"{failureReason} at position {failureOffset}");
        }

        return new FlexibleFormatter(format, [.. segments], allocatorType, stackAllocBufferSize);
    }

    /// <summary>
    ///     Parse format string with single-character delimiters: %name%, $name$, etc.
    /// </summary>
    private static FlexibleFormatter ParseDelimited(string format, char open, char close, AllocatorType allocatorType, int stackAllocBufferSize)
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

        return new FlexibleFormatter(format, segments.ToArray(), allocatorType, stackAllocBufferSize);
    }

    /// <summary>
    ///     Parse format string with multi-character custom delimiters.
    /// </summary>
    private static FlexibleFormatter ParseDelimitedString(string format, string open, string close, AllocatorType allocatorType, int stackAllocBufferSize)
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

        return new FlexibleFormatter(format, segments.ToArray(), allocatorType, stackAllocBufferSize);
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
