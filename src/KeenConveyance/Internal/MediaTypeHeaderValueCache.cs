using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace KeenConveyance.Internal;

/// <summary>
/// <see cref="string"/> 对应 <see cref="MediaTypeHeaderValue"/> 的缓存
/// </summary>
internal static class MediaTypeHeaderValueCache
{
    #region 静态 Private 字段

    private static readonly ConditionalWeakTable<string, MediaTypeHeaderValue> s_mediaTypeHeaderValueCache = [];

    #endregion 静态 Private 字段

    #region Public 方法

    /// <summary>
    /// 获取 <paramref name="mediaType"/> 对应的已缓存的 <see cref="MediaTypeHeaderValue"/>
    /// </summary>
    /// <param name="mediaType"></param>
    /// <returns></returns>
    public static MediaTypeHeaderValue GetCached(string mediaType)
    {
        if (!s_mediaTypeHeaderValueCache.TryGetValue(mediaType, out var cachedValue))
        {
            cachedValue = new MediaTypeHeaderValue(mediaType);

            s_mediaTypeHeaderValueCache.AddOrUpdate(mediaType, cachedValue);
        }
        return cachedValue;
    }

    #endregion Public 方法
}
