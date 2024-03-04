using System.Net;
using KeenConveyance.Internal;

namespace KeenConveyance.Client;

/// <summary>
/// 内容由 <see cref="IBufferProvider"/> 提供的 <see cref="HttpContent"/>
/// </summary>
public sealed class BufferHttpContent : HttpContent
{
    #region Private 字段

    /// <inheritdoc cref="IBufferProvider"/>
    private readonly IBufferProvider _bufferProvider;

    /// <summary>
    /// 用于取消写入的 <see cref="CancellationToken"/>
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion Private 字段

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="BufferHttpContent"/>
    /// </summary>
    /// <param name="mediaType"></param>
    /// <param name="bufferProvider"></param>
    /// <param name="cancellationToken"></param>
    public BufferHttpContent(string mediaType, IBufferProvider bufferProvider, CancellationToken cancellationToken)
    {
        _bufferProvider = bufferProvider ?? throw new ArgumentNullException(nameof(bufferProvider));
        _cancellationToken = cancellationToken;

        Headers.ContentType = MediaTypeHeaderValueCache.GetCached(mediaType);
    }

    #endregion Public 构造函数

    #region Protected 方法

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _bufferProvider.Dispose();
    }

    /// <inheritdoc/>
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var writeTask = stream.WriteAsync(_bufferProvider.Buffer, _cancellationToken);
        return writeTask.AsTask();
    }

    /// <inheritdoc/>
    protected override bool TryComputeLength(out long length)
    {
        length = _bufferProvider.Length;
        return true;
    }

    #endregion Protected 方法
}
