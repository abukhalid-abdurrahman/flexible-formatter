namespace FlexibleFormatter;

/// <summary>
/// Specifies the memory allocation strategy for string formatting.
/// </summary>
public enum AllocatorType
{
    /// <summary>
    /// Use heap allocation with StringBuilder (default, safer for long strings).
    /// </summary>
    Heap = 0,

    /// <summary>
    /// Use stack allocation with DefaultInterpolatedStringHandler (faster, but limited buffer size).
    /// </summary>
    StackAlloc = 1
}
