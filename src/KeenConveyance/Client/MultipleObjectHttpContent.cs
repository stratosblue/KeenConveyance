using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using KeenConveyance.Serialization;

namespace KeenConveyance.Client;

/// <summary>
/// 多对象的<see cref="HttpContent"/>
/// </summary>
public abstract class MultipleObjectHttpContent : HttpContent
{
    #region 静态 Private 字段

    private static readonly ConditionalWeakTable<string, MediaTypeHeaderValueCache> s_mediaTypeHeaderValueCache = [];

    #endregion 静态 Private 字段

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

        Headers.ContentType = GetCachedMediaTypeHeaderValue(objectSerializer.SupportedMediaType.MediaType);
    }

    #endregion Public 构造函数

    #region 静态 Protected 方法

    /// <summary>
    /// 获取 <paramref name="mediaType"/> 对应的已缓存的 <see cref="MediaTypeHeaderValue"/>
    /// </summary>
    /// <param name="mediaType"></param>
    /// <returns></returns>
    protected static MediaTypeHeaderValue GetCachedMediaTypeHeaderValue(string mediaType)
    {
        if (!s_mediaTypeHeaderValueCache.TryGetValue(mediaType, out var cachedValue))
        {
            cachedValue = new()
            {
                Value = new MediaTypeHeaderValue(mediaType)
            };
            s_mediaTypeHeaderValueCache.AddOrUpdate(mediaType, cachedValue);
        }
        return cachedValue.Value!;
    }

    #endregion 静态 Protected 方法

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
    protected abstract Task WriteContentAsync(IMultipleObjectStreamSerializer serializer);

    #endregion Protected 方法

    #region Private 类

    private sealed class MediaTypeHeaderValueCache
    {
        #region Public 属性

        public MediaTypeHeaderValue? Value { get; set; }

        #endregion Public 属性
    }

    #endregion Private 类
}
