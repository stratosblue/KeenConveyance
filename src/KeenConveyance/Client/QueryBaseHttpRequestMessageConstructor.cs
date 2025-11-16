using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace KeenConveyance.Client;

/// <summary>
/// 基于Query的 <see cref="IHttpRequestMessageConstructor"/>
/// </summary>
public sealed class QueryBaseHttpRequestMessageConstructor : IHttpRequestMessageConstructor
{
    #region Private 字段

    private static readonly ConditionalWeakTable<Uri, ConcurrentDictionary<string, Uri>> s_uriCache = [];

    private readonly string _queryKey;

    private readonly string _serviceEntryPath;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="QueryBaseHttpRequestMessageConstructor"/>
    public QueryBaseHttpRequestMessageConstructor(string serviceEntryPath = KeenConveyanceConstants.DefaultEntryPath, string queryKey = QueryBaseEntryKeyOptions.DefaultQueryKey)
    {
        if (string.IsNullOrWhiteSpace(serviceEntryPath))
        {
            throw new ArgumentException($"“{nameof(serviceEntryPath)}”不能为 null 或空白。", nameof(serviceEntryPath));
        }

        if (string.IsNullOrWhiteSpace(queryKey))
        {
            throw new ArgumentException($"“{nameof(queryKey)}”不能为 null 或空白。", nameof(queryKey));
        }

        _serviceEntryPath = serviceEntryPath;
        _queryKey = queryKey;
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
                Path = _serviceEntryPath,
                Query = $"?{_queryKey}={entryKey}",
            };
            requestUri = builder.Uri;
            uriDictionary.TryAdd(entryKey, requestUri);
        }
        return requestUri;
    }

    #endregion Private 方法

    #region Public 方法

    /// <inheritdoc/>
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
