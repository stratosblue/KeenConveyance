namespace KeenConveyance;

/// <summary>
/// KeenConveyance 异常
/// </summary>
public class KeenConveyanceException : Exception
{
    #region Public 构造函数

    /// <inheritdoc/>
    public KeenConveyanceException()
    { }

    /// <inheritdoc/>
    public KeenConveyanceException(string message) : base(message) { }

    /// <inheritdoc/>
    public KeenConveyanceException(string message, Exception inner) : base(message, inner) { }

    #endregion Public 构造函数
}
