using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using FlexibleFormatter.Resources;

namespace FlexibleFormatter;

/// <summary>
///     Represents a parsed flexible format string with support for multiple parameter styles.
/// </summary>
[DebuggerDisplay("{Template}")]
public sealed partial class FlexibleFormatter
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

    /// <summary>
    ///     The memory allocation strategy for formatting.
    /// </summary>
    private readonly AllocatorType _allocatorType;

    /// <summary>
    ///     The buffer size for stack allocation.
    /// </summary>
    private readonly int _stackAllocBufferSize;

    /// <summary>
    ///     Default buffer size for stack allocation (256 chars should be enough for most scenarios).
    /// </summary>
    internal const int DefaultStackAllocBufferSize = 256;

    /// <summary>
    ///     Maximum buffer size for stack allocation (32KB limit).
    /// </summary>
    internal const int MaxStackAllocBufferSize = 32768;

    private FlexibleFormatter(string format,
        (string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format)[] segments,
        AllocatorType allocatorType = AllocatorType.Heap,
        int stackAllocBufferSize = DefaultStackAllocBufferSize)
    {
        Debug.Assert(format is not null);

        ValidateBufferSize(stackAllocBufferSize);

        Template = format;
        _segments = segments;
        _allocatorType = allocatorType;
        _stackAllocBufferSize = stackAllocBufferSize;

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
            throw new FormatException(ExceptionMessages.FormatRequiresAtLeastArgument(_argsRequired, args.Length));

        return FormatCore(args, namedArgs: null);
    }

    /// <summary>
    ///     Format using 1 indexed argument.
    /// </summary>
    public string Format(object? arg0)
    {
        return FormatCore(indexedArgs: [arg0], namedArgs: null);
    }

    /// <summary>
    ///     Format using 2 indexed arguments.
    /// </summary>
    public string Format(object? arg0, object? arg1)
    {
        return FormatCore(indexedArgs: [arg0, arg1], namedArgs: null);
    }

    /// <summary>
    ///     Format using 3 indexed arguments.
    /// </summary>
    public string Format(object? arg0, object? arg1, object? arg2)
    {
        return FormatCore(indexedArgs: [arg0, arg1, arg2], namedArgs: null);
    }

    /// <summary>
    ///     Format using 4 indexed arguments.
    /// </summary>
    public string Format(object? arg0, object? arg1, object? arg2, object? arg3)
    {
        return FormatCore(indexedArgs: [arg0, arg1, arg2, arg3], namedArgs: null);
    }

    /// <summary>
    ///     Format using 5 indexed arguments.
    /// </summary>
    public string Format(object? arg0, object? arg1, object? arg2, object? arg3, object? arg4)
    {
        return FormatCore(indexedArgs: [arg0, arg1, arg2, arg3, arg4], namedArgs: null);
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
            throw new FormatException(ExceptionMessages.FormatRequiresAtLeastArgument(_argsRequired, indexedArgs.Length));

        return FormatCore(indexedArgs, namedArgs);
    }

    /// <summary>Core formatting implementation.</summary>
    private string FormatCore(object?[]? indexedArgs, Dictionary<string, object?>? namedArgs)
    {
        if (_allocatorType is AllocatorType.StackAlloc)
        {
            DefaultInterpolatedStringHandler handler = new(
                _literalLength, _formattedCount, provider: null, initialBuffer: stackalloc char[_stackAllocBufferSize]);

            foreach ((string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format) segment in _segments)
            {
                if (segment.Literal is string literal)
                {
                    handler.AppendLiteral(literal);
                }
                else
                {
                    // Get the value to format
                    object? value;
                    if (segment.ParamName is not null)
                    {
                        if (namedArgs is null || !namedArgs.TryGetValue(segment.ParamName, out value))
                        {
                            throw new FormatException($"Named parameter '{segment.ParamName}' was not provided");
                        }
                    }
                    else
                    {
                        if (indexedArgs is null || segment.ArgIndex < 0 || segment.ArgIndex >= indexedArgs.Length)
                        {
                            throw new FormatException($"Index {segment.ArgIndex} is out of range");
                        }
                        value = indexedArgs[segment.ArgIndex];
                    }

                    handler.AppendFormatted(value, segment.Alignment, segment.Format);
                }
            }

            return handler.ToStringAndClear();
        }
        else
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
                    if (segment.ParamName is not null)
                    {
                        // Named parameter
                        if (namedArgs is null || !namedArgs.TryGetValue(segment.ParamName, out value))
                        {
                            throw new FormatException($"Named parameter '{segment.ParamName}' was not provided");
                        }
                    }
                    else
                    {
                        // Indexed parameter
                        if (indexedArgs is null || segment.ArgIndex < 0 || segment.ArgIndex >= indexedArgs.Length)
                        {
                            throw new FormatException($"Index {segment.ArgIndex} is out of range");
                        }
                        value = indexedArgs[segment.ArgIndex];
                    }

                    AppendFormatted(sb, value, segment.Alignment, segment.Format);
                }
            }

            return sb.ToString();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendFormatted<T>(StringBuilder sb, T value, int alignment, string? format)
    {
        string formattedValue;
        if (format != null && value is IFormattable formattable)
        {
            formattedValue = formattable.ToString(format, null);
        }
        else
        {
            formattedValue = value?.ToString() ?? string.Empty;
        }

        if (alignment != 0)
        {
            int width = Math.Abs(alignment);
            if (formattedValue.Length < width)
            {
                if (alignment > 0)
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

    private static void ValidateBufferSize(int bufferSize)
    {
        if (bufferSize < 0 || bufferSize > MaxStackAllocBufferSize)
            throw new ArgumentOutOfRangeException(nameof(bufferSize),
                ExceptionMessages.BufferSizeMustBeBetweenZeroAnd(MaxStackAllocBufferSize));
    }
}
