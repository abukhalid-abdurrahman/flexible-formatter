namespace FlexibleFormatter;

/// <summary>
///     Defines the parameter style for formatting.
/// </summary>
public enum ParameterStyle
{
    /// <summary>
    ///     Standard composite format: {0}, {1:format}, {name}
    /// </summary>
    Braces,

    /// <summary>
    ///     Percent-delimited format: %UserName%, %Value%
    /// </summary>
    Percent,

    /// <summary>
    ///     Dollar-delimited format: $UserName$, $Value$
    /// </summary>
    Dollar,

    /// <summary>
    ///     Custom delimiters defined by user
    /// </summary>
    Custom
}
