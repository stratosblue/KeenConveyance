using System.Net;
using System.Text;
using KeenConveyance.Internal;
using KeenConveyance.Serialization;

namespace KeenConveyance.Client;

/// <summary>
/// 多对象的<see cref="HttpContent"/>
/// </summary>
public abstract class MultipleObjectHttpContent : KeenConveyanceHttpContentBase
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

    #region Public 方法

    /// <inheritdoc/>
    public override object? ToTagValue()
    {
        using var bufferWriter = new PooledBufferWriter(IActivityTagable.BufferInitialCapacity);
        using (var streamSerializer = IActivityTagable.TagValueJsonObjectSerializer.CreateObjectStreamSerializer(bufferWriter))
        {
            SerializeContent(streamSerializer, CancellationToken.None);
        }
        return Encoding.UTF8.GetString(bufferWriter.Buffer.Span);
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 将内容序列化到 <paramref name="serializer"/>
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected void SerializeContent(IMultipleObjectStreamSerializer serializer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        serializer.Start();

        WriteContent(serializer, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        serializer.Finish();
    }

    /// <summary>
    /// 将内容序列化到 <paramref name="serializer"/>
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task SerializeContentAsync(IMultipleObjectAsyncStreamSerializer serializer, CancellationToken cancellationToken)
    {
        await serializer.StartAsync(cancellationToken).ConfigureAwait(false);
        await WriteContentAsync(serializer, cancellationToken).ConfigureAwait(false);
        await serializer.FinishAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        CancellationToken.ThrowIfCancellationRequested();

        using var streamSerializer = ObjectSerializer.CreateObjectStreamSerializer(stream);
        await SerializeContentAsync(streamSerializer, CancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override bool TryComputeLength(out long length)
    {
        length = default;
        return false;
    }

    /// <summary>
    /// 将 数据 写入 <paramref name="serializer"/>
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract void WriteContent(IMultipleObjectStreamSerializer serializer, CancellationToken cancellationToken);

    /// <summary>
    /// 将 数据 写入 <paramref name="serializer"/>
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task WriteContentAsync(IMultipleObjectAsyncStreamSerializer serializer, CancellationToken cancellationToken);

    #endregion Protected 方法
}
