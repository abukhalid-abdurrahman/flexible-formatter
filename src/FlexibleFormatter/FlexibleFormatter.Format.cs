using System.Runtime.CompilerServices;
using System.Text;
using FlexibleFormatter.Resources;

namespace FlexibleFormatter;

public sealed partial class FlexibleFormatter
{
    /// <summary>
    ///     Format using set of named parameter.
    /// </summary>
    public string FormatNamed(ReadOnlySpan<string> names, ReadOnlySpan<object?> args)
    {
        return FormatNamedInternal(names, args);
    }

    /// <summary>
    ///     Format using 1 named parameter.
    /// </summary>
    public string FormatNamed<TArg0>(string name0, TArg0 arg0)
    {
        return FormatNamedInternal(
            names: [name0],
            args: [arg0]
        );
    }

    /// <summary>
    ///     Format using 2 named parameters.
    /// </summary>
    public string FormatNamed<TArg0, TArg1>(string name0, TArg0 arg0, string name1, TArg1 arg1)
    {
        return FormatNamedInternal(
            names: [name0, name1],
            args: [arg0, arg1]
        );
    }

    /// <summary>
    ///     Format using 3 named parameters.
    /// </summary>
    public string FormatNamed<TArg0, TArg1, TArg2>(
        string name0, TArg0 arg0,
        string name1, TArg1 arg1,
        string name2, TArg2 arg2)
    {
        return FormatNamedInternal(
            names: [name0, name1, name2],
            args: [arg0, arg1, arg2]
        );
    }

    /// <summary>
    ///     Format using 4 named parameters.
    /// </summary>
    public string FormatNamed<TArg0, TArg1, TArg2, TArg3>(
        string name0, TArg0 arg0,
        string name1, TArg1 arg1,
        string name2, TArg2 arg2,
        string name3, TArg3 arg3)
    {
        return FormatNamedInternal(
            names: [name0, name1, name2, name3],
            args: [arg0, arg1, arg2, arg3]
        );
    }

    /// <summary>
    ///     Format using 5 named parameters.
    /// </summary>
    public string FormatNamed<TArg0, TArg1, TArg2, TArg3, TArg4>(
        string name0, TArg0 arg0,
        string name1, TArg1 arg1,
        string name2, TArg2 arg2,
        string name3, TArg3 arg3,
        string name4, TArg4 arg4)
    {
        return FormatNamedInternal(
            names: [name0, name1, name2, name3, name4],
            args: [arg0, arg1, arg2, arg3, arg4]
        );
    }

    /// <summary>
    ///     Format using 6 named parameters.
    /// </summary>
    public string FormatNamed<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5>(
        string name0, TArg0 arg0,
        string name1, TArg1 arg1,
        string name2, TArg2 arg2,
        string name3, TArg3 arg3,
        string name4, TArg4 arg4,
        string name5, TArg5 arg5)
    {
        return FormatNamedInternal(
            names: [name0, name1, name2, name3, name4, name5],
            args: [arg0, arg1, arg2, arg3, arg4, arg5]
        );
    }

    /// <summary>
    ///     Format using 7 named parameters.
    /// </summary>
    public string FormatNamed<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
        string name0, TArg0 arg0,
        string name1, TArg1 arg1,
        string name2, TArg2 arg2,
        string name3, TArg3 arg3,
        string name4, TArg4 arg4,
        string name5, TArg5 arg5,
        string name6, TArg6 arg6)
    {
        return FormatNamedInternal(
            names: [name0, name1, name2, name3, name4, name5, name6],
            args: [arg0, arg1, arg2, arg3, arg4, arg5, arg6]
        );
    }

    private string FormatNamedInternal(ReadOnlySpan<string> names, ReadOnlySpan<object?> args)
    {
        if (_formattedCount is 0)
        {
            return Template;
        }

        if (_allocatorType is AllocatorType.StackAlloc)
        {
            DefaultInterpolatedStringHandler handler = new(
                _literalLength, _formattedCount, provider: null, initialBuffer: stackalloc char[_stackAllocBufferSize]);
            FormatNamedCore(ref handler, names, args);
            return handler.ToStringAndClear();
        }
        else
        {
            int capacity = _literalLength + _formattedCount * 8;
            StringBuilder sb = new(capacity);
            FormatNamedCore(sb, names, args);
            return sb.ToString();
        }
    }

    private void FormatNamedCore(ref DefaultInterpolatedStringHandler handler, scoped ReadOnlySpan<string> names, scoped ReadOnlySpan<object?> args)
    {
        ValidateNamedParameters(ref names, ref args);

        for (int i = 0; i < _segments.Length; i++)
        {
            (string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format) segment = _segments[i];
            if (segment.Literal is string literal)
            {
                handler.AppendLiteral(literal);
            }
            else if (segment.ParamName is not null && names.IndexOf(segment.ParamName) is int nameIndex)
            {
                if (nameIndex < 0)
                    throw new FormatException($"Named parameter '{segment.ParamName}' was not provided");

                handler.AppendFormatted(args[nameIndex], segment.Alignment, segment.Format);
            }
            else
            {
                throw new FormatException($"Indexed parameter {segment.ArgIndex} cannot be used with named parameter methods");
            }
        }
    }

    private void FormatNamedCore(StringBuilder sb, scoped ReadOnlySpan<string> names, scoped ReadOnlySpan<object?> args)
    {
        ValidateNamedParameters(ref names, ref args);

        for (int i = 0; i < _segments.Length; i++)
        {
            (string? Literal, string? ParamName, int ArgIndex, int Alignment, string? Format) segment = _segments[i];
            if (segment.Literal is string literal)
            {
                sb.Append(literal);
            }
            else if (segment.ParamName is not null && names.IndexOf(segment.ParamName) is int nameIndex)
            {
                if (nameIndex < 0)
                    throw new FormatException($"Named parameter '{segment.ParamName}' was not provided");

                AppendFormatted(sb, args[nameIndex], segment.Alignment, segment.Format);
            }
            else
            {
                throw new FormatException($"Indexed parameter {segment.ArgIndex} cannot be used with named parameter methods");
            }
        }
    }

    private static void ValidateNamedParameters(ref ReadOnlySpan<string> names, ref ReadOnlySpan<object?> args)
    {
        if (args.IsEmpty)
            throw new FormatException(ExceptionMessages.NoArgumentsWereProvided);

        if (names.IsEmpty)
            throw new FormatException(ExceptionMessages.NoNamesWereProvided);

        if (names.Length != args.Length)
            throw new FormatException(ExceptionMessages.NumberOfNamesNotMatchNumberOfArguments(names.Length, args.Length));
    }
}
