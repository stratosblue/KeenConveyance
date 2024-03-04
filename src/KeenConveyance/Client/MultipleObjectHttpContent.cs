using System.Net;
using KeenConveyance.Internal;
using KeenConveyance.Serialization;

namespace KeenConveyance.Client;

/// <summary>
/// 多对象的<see cref="HttpContent"/>
/// </summary>
public abstract class MultipleObjectHttpContent : HttpContent
{
    #region Private 字段

    /// <summary>
    /// 用于取消写入的 <see cref="System.Threading.CancellationToken"/>
    /// </summary>
    protected readonly CancellationToken CancellationToken;

    /// <summary>
    /// 用于序列化对象的<see cref="IObjectSerializer"/>
    /// </summary>
    protected readonly IObjectSerializer ObjectSerializer;

    #endregion Private 字段

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="MultipleObjectHttpContent"/>
    /// </summary>
    /// <param name="objectSerializer"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MultipleObjectHttpContent(IObjectSerializer objectSerializer, CancellationToken cancellationToken)
    {
        ObjectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));
        CancellationToken = cancellationToken;

        Headers.ContentType = MediaTypeHeaderValueCache.GetCached(objectSerializer.SupportedMediaType.MediaType);
    }

    #endregion Public 构造函数

    #region Protected 方法

    /// <inheritdoc/>
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        CancellationToken.ThrowIfCancellationRequested();

        using var streamSerializer = ObjectSerializer.CreateObjectStreamSerializer(stream);
        await streamSerializer.StartAsync(CancellationToken).ConfigureAwait(false);
        await WriteContentAsync(streamSerializer).ConfigureAwait(false);
        await streamSerializer.FinishAsync(CancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override bool TryComputeLength(out long length)
    {
        length = default;
        return false;
    }

    /// <summary>
    /// 将 数据 写入请求
    /// </summary>
    /// <param name="serializer"></param>
    /// <returns></returns>
    protected abstract Task WriteContentAsync(IMultipleObjectAsyncStreamSerializer serializer);

    #endregion Protected 方法
}
