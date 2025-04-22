using System.ComponentModel;

namespace KeenConveyance.Client;

/// <summary>
/// KeenConveyance 的 <see cref="HttpContent"/> 基类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class KeenConveyanceHttpContentBase : HttpContent, IActivityTagable
{
    #region Public 方法

    /// <inheritdoc/>
    public virtual object? ToTagValue() => null;

    #endregion Public 方法
}
