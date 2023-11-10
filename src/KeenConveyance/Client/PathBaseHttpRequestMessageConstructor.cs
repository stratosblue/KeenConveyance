using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace KeenConveyance;

internal sealed class PathBaseHttpRequestMessageConstructor : IHttpRequestMessageConstructor
{
    #region Private 字段

    private static readonly ConditionalWeakTable<Uri, ConcurrentDictionary<string, Uri>> s_uriCache = new();

    private readonly string _serviceEntryPath;

    #endregion Private 字段

    #region Public 构造函数

    public PathBaseHttpRequestMessageConstructor(string serviceEntryPath)
    {
        if (string.IsNullOrWhiteSpace(serviceEntryPath))
        {
            throw new ArgumentException($"“{nameof(serviceEntryPath)}”不能为 null 或空白。", nameof(serviceEntryPath));
        }

        _serviceEntryPath = serviceEntryPath;
    }

    #endregion Public 构造函数

    #region Private 方法

    private Uri GetRequestUri(Uri serviceAddress, string entryKey)
    {
        var uriDictionary = s_uriCache.GetOrCreateValue(serviceAddress);

        if (!uriDictionary.TryGetValue(entryKey, out var requestUri))
        {
            var builder = new UriBuilder(serviceAddress)
            {
                Path = $"{_serviceEntryPath}/{entryKey}",
            };
            requestUri = builder.Uri;
            uriDictionary.TryAdd(entryKey, requestUri);
        }
        return requestUri;
    }

    #endregion Private 方法

    #region Public 方法

    public HttpRequestMessage CreateHttpRequestMessage(Uri serviceAddress, string entryKey, HttpContent? httpContent)
    {
        var requestUri = GetRequestUri(serviceAddress, entryKey);

        return new HttpRequestMessage(HttpMethod.Post, requestUri: requestUri)
        {
            Content = httpContent,
        };
    }

    #endregion Public 方法
}
