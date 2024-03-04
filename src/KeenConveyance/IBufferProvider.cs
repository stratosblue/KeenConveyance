namespace KeenConveyance;

/// <summary>
/// Buffer提供器
/// </summary>
public interface IBufferProvider : IDisposable
{
    #region Public 属性

    /// <summary>
    /// 获取Buffer
    /// </summary>
    ReadOnlyMemory<byte> Buffer { get; }

    /// <summary>
    /// 获取Buffer长度
    /// </summary>
    int Length { get; }

    #endregion Public 属性
}
