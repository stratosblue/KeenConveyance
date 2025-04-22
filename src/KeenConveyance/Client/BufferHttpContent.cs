using System.Net;
using System.Text;
using KeenConveyance.Internal;

namespace KeenConveyance.Client;

/// <summary>
/// 内容由 <see cref="IBufferProvider"/> 提供的 <see cref="HttpContent"/>
/// </summary>
public sealed class BufferHttpContent : KeenConveyanceHttpContentBase
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

    #region Public 方法

    /// <inheritdoc/>
    public override object? ToTagValue()
    {
        //仅在此类内容时提供值
        //TODO缓存
        if (Headers.ContentType?.MediaType is { } mediaType
            && (mediaType.Contains("text", StringComparison.OrdinalIgnoreCase)
                || mediaType.Contains("json", StringComparison.OrdinalIgnoreCase)
                || mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                return Encoding.UTF8.GetString(_bufferProvider.Buffer.Span);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error at create activity tag value: {ex.Message}");
            }
        }
        return null;
    }

    #endregion Public 方法

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
