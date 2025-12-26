using System.Resources;
using DotNext.Resources;

namespace FlexibleFormatter.Resources;

internal static class ExceptionMessages
{
    private static readonly ResourceManager _resources = new(typeof(ExceptionMessages).FullName!, typeof(ExceptionMessages).Assembly!);

    internal static string UseParseCustomForCustomDelimiters = _resources.Get().AsString();
    internal static string DelimitersCannotBeEmpty = _resources.Get().AsString();
    internal static string NoArgumentsWereProvided = _resources.Get().AsString();
    internal static string NoNamesWereProvided = _resources.Get().AsString();

    internal static string BufferSizeMustBeBetweenZeroAnd(int maxBufferSize) =>
        _resources.Get().Format(maxBufferSize);

    internal static string FormatRequiresAtLeastArgument(int expectedArgs, int actualArgs) =>
        _resources.Get().Format(expectedArgs, actualArgs);

    internal static string NumberOfNamesNotMatchNumberOfArguments(int namesCount, int argsCount) =>
        _resources.Get().Format(namesCount, argsCount);
}
