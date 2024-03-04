namespace KeenConveyance.Serialization;

/// <summary>
/// 多对象异步流式序列化器
/// </summary>
public interface IMultipleObjectAsyncStreamSerializer : IDisposable
{
    #region Public 方法

    /// <summary>
    /// 完成写入
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask FinishAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 开始写入
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 写入 <paramref name="value"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask WriteAsync<T>(T? value, CancellationToken cancellationToken);

    /// <summary>
    /// 写入 <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask WriteAsync(object? value, Type type, CancellationToken cancellationToken);

    #endregion Public 方法
}
